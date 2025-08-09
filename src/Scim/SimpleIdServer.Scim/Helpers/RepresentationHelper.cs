﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.Extensions.Options;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.DTOs;
using SimpleIdServer.Scim.Exceptions;
using SimpleIdServer.Scim.Extensions;
using SimpleIdServer.Scim.Parser;
using SimpleIdServer.Scim.Parser.Expressions;
using SimpleIdServer.Scim.Persistence;
using SimpleIdServer.Scim.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleIdServer.Scim.Helpers
{
    public interface IRepresentationHelper
    {
        Task<SCIMRepresentationPatchResult> Apply(SCIMRepresentation representation, IEnumerable<PatchOperationParameter> patchLst, IEnumerable<SCIMAttributeMapping> attributeMappings, bool ignoreUnsupportedCanonicalValues, CancellationToken cancellationToken);
        Task CheckUniqueness(string realm, IEnumerable<SCIMRepresentationAttribute> attributes);
        void CheckMutability(List<SCIMPatchResult> patchOperations);
        SCIMRepresentation ExtractSCIMRepresentationFromJSON(JsonObject json, string externalId, SCIMSchema mainSchema, ICollection<SCIMSchema> extensionSchemas, IEnumerable<SCIMAttributeMapping> attributeMappings);
    }

    public class RepresentationHelper : IRepresentationHelper
    {
        private readonly ISCIMRepresentationCommandRepository _scimRepresentationCommandRepository;
        public readonly ScimHostOptions _options;

        public RepresentationHelper(ISCIMRepresentationCommandRepository scimRepresentationCommandRepository, IOptions<ScimHostOptions> options)
        {
            _scimRepresentationCommandRepository = scimRepresentationCommandRepository;
            _options = options.Value;
        }

        #region Apply patch operations

        public async Task<SCIMRepresentationPatchResult> Apply(SCIMRepresentation representation, IEnumerable<PatchOperationParameter> patchLst, IEnumerable<SCIMAttributeMapping> attributeMappings, bool ignoreUnsupportedCanonicalValues, CancellationToken cancellationToken)
        {
            var attrSelectors = attributeMappings.Select(a => a.SourceAttributeSelector).Distinct();
            var sanitizedPatchOperations = SanitizePatchOperations(representation, patchLst);
            var result = new SCIMRepresentationPatchResult();
            foreach (var rec in sanitizedPatchOperations)
            {
                SCIMAttributeExpression scimExpr = null;
                var patch = rec.Item1;
                var scimFilter = rec.Item2;
                var schemaAttributes = representation.Schemas.SelectMany(_ => _.Attributes);
                List<SCIMRepresentationAttribute> filteredAttributes = null, hierarchicalNewAttributes = null, hierarchicalFilteredAttributes = null;
                string fullPath = null;
                SCIMSchemaAttribute scimExprSchemaAttr = null;

                if(scimFilter != null)
                {
                    scimExpr = scimFilter as SCIMAttributeExpression;
                    if (scimExpr == null) throw new SCIMAttributeException(Global.InvalidAttributeExpression);
                    scimExprSchemaAttr = scimExpr.GetLastChild().SchemaAttribute;
                    fullPath = scimExpr.GetFullPath();
                    schemaAttributes = representation.Schemas.Select(s => s.GetAttribute(fullPath)).Where(s => s != null);
                    fullPath = SCIMAttributeExpression.RemoveNamespace(fullPath);
                }

                if (patch.Operation != SCIMPatchOperations.REMOVE)
                {
                    var attributes = ExtractRepresentationAttributesFromJSON(representation.Schemas, schemaAttributes.ToList(), patch.Value, ignoreUnsupportedCanonicalValues);
                    attributes = RemoveStandardReferenceProperties(attributes, attributeMappings);
                    hierarchicalNewAttributes = SCIMRepresentation.BuildHierarchicalAttributes(attributes);
                    if (scimFilter != null && attributes != null && !attributes.Any(a => a.IsLeaf()))
                    {
                        var lst = new List<SCIMRepresentationAttribute>();
                        foreach (var hNewAttribute in hierarchicalNewAttributes)
                        {
                            var record = scimFilter.BuildEmptyAttributes().FirstOrDefault();
                            record.UpdateValue(hNewAttribute.FullPath, hNewAttribute);
                            lst.Add(record);
                        }

                        hierarchicalNewAttributes = lst;
                    }

                    foreach (var h in hierarchicalNewAttributes)
                    {
                        h.ComputeValueIndex();
                    }
                }

                if (patch.Operation == SCIMPatchOperations.ADD && !(scimFilter is SCIMComplexAttributeExpression) && attrSelectors.Contains(fullPath))
                {
                    var computedValueIndexLst = hierarchicalNewAttributes.Select(n => n.ComputedValueIndex).ToList();
                    var existingAttrs = await _scimRepresentationCommandRepository.FindAttributesByComputedValueIndexAndRepresentationId(computedValueIndexLst, representation.Id, cancellationToken);
                    var hierarchicalExistingAttrs = SCIMRepresentation.BuildHierarchicalAttributes(existingAttrs);
                    hierarchicalNewAttributes = FilterDuplicate(hierarchicalExistingAttrs, hierarchicalNewAttributes);
                    foreach(var newAttr in hierarchicalNewAttributes)
                    {
                        TryInsertComplexMultivaluedAttribute(result, newAttr, null, scimExprSchemaAttr, representation);
                    }

                    continue;
                }

                if (scimFilter != null)
                {
                    filteredAttributes = await _scimRepresentationCommandRepository.FindAttributes(representation.Id, scimExpr, cancellationToken);
                    if(hierarchicalNewAttributes != null && !filteredAttributes.Any())
                    {
                        hierarchicalFilteredAttributes = scimFilter.EvaluateAttributes(SCIMRepresentation.BuildHierarchicalAttributes(result.Patches.Select(p => p.Attr)).AsQueryable(), false).ToList();
                        filteredAttributes = SCIMRepresentation.BuildFlatAttributes(hierarchicalNewAttributes);
                    }
                    else
                    {
                        hierarchicalFilteredAttributes = SCIMRepresentation.BuildHierarchicalAttributes(filteredAttributes);
                    }

                    var complexAttr = scimFilter as SCIMComplexAttributeExpression;
                    if (complexAttr != null && !hierarchicalFilteredAttributes.Any() && complexAttr.GroupingFilter != null && patch.Operation == SCIMPatchOperations.REPLACE) throw new SCIMNoTargetException(Global.PatchMissingAttribute);
                }


                var computedIndexes = hierarchicalNewAttributes?.Select(a => a.ComputedValueIndex)?.ToList() ?? new List<string>();
                if(hierarchicalFilteredAttributes != null && hierarchicalNewAttributes != null) hierarchicalNewAttributes = FilterDuplicate(hierarchicalFilteredAttributes, hierarchicalNewAttributes);

                var removeCallback = new Action<ICollection<SCIMRepresentationAttribute>>((attrs) =>
                {
                    foreach (var a in attrs)
                    {
                        result.Remove(a);
                    }
                });

                switch (patch.Operation)
                {
                    case SCIMPatchOperations.ADD:
                        {
                            UpdateCommonAttributes(patch, representation, result, schemaAttributes);
                            if (hierarchicalFilteredAttributes == null)
                            {
                                var complexMultiValuedAttributes = hierarchicalNewAttributes.Where(a => a.SchemaAttribute.MultiValued && a.SchemaAttribute.Type == SCIMSchemaAttributeTypes.COMPLEX);
                                var standardMultiValuedAttributes = hierarchicalNewAttributes.Where(a => a.SchemaAttribute.MultiValued && a.SchemaAttribute.Type != SCIMSchemaAttributeTypes.COMPLEX);
                                var notMultiValuedAttributes = hierarchicalNewAttributes.Where(a => !a.SchemaAttribute.MultiValued).ToList();
                                foreach(var multiValuedAttribute in complexMultiValuedAttributes)
                                {
                                    var arrayParentAttribute = filteredAttributes.FirstOrDefault(f => f.FullPath == multiValuedAttribute.FullPath);
                                    if (TryInsertComplexMultivaluedAttribute(result, multiValuedAttribute, arrayParentAttribute, null, representation)) continue;
                                }

                                foreach(var standardMultiValuedAttribute in standardMultiValuedAttributes)
                                    if (TryInsertValueIntoArray(result, standardMultiValuedAttribute, null, representation)) continue;

                                await OverrideRootAttributes(result, notMultiValuedAttributes, representation, cancellationToken);
                            }
                            else
                            {
                                
                                SCIMRepresentationAttribute arrayParentAttribute = null;
                                if (hierarchicalFilteredAttributes.Any() && hierarchicalNewAttributes.Any() && scimExprSchemaAttr.MultiValued && !SCIMRepresentationAttribute.IsLeaf(fullPath))
                                {
                                    var parentFullPath = SCIMRepresentationAttribute.GetParentFullPath(fullPath);
                                    arrayParentAttribute = filteredAttributes.First(f => f.FullPath == parentFullPath);
                                }

                                foreach (var newAttribute in hierarchicalNewAttributes)
                                {
                                    if (TryInsertComplexMultivaluedAttribute(result, newAttribute, arrayParentAttribute, scimExprSchemaAttr, representation)) continue;
                                    if (TryInsertValueIntoArray(result, newAttribute, scimExprSchemaAttr, representation)) continue;
                                    if (TryInsertWhenTargetLocationIsUnknown(result, newAttribute, hierarchicalFilteredAttributes, representation)) continue;
                                    ReplaceAttributes(result, newAttribute, hierarchicalFilteredAttributes, filteredAttributes, representation);
                                }
                            }
                        }
                        break;
                    case SCIMPatchOperations.REMOVE:
                        {
                            if (scimFilter == null) throw new SCIMNoTargetException(string.Format(Global.InvalidPath, patch.Path));
                            if (SCIMFilterParser.DontContainsFilter(patch.Path) && patch.Value != null)
                            {
                                var excludedAttributes = ExtractRepresentationAttributesFromJSON(representation.Schemas, schemaAttributes.ToList(), patch.Value, ignoreUnsupportedCanonicalValues);
                                excludedAttributes = RemoveStandardReferenceProperties(excludedAttributes, attributeMappings);
                                excludedAttributes = SCIMRepresentation.BuildHierarchicalAttributes(excludedAttributes);
                                filteredAttributes = filteredAttributes.Where(a => excludedAttributes.Any(ea => ea.IsSimilar(a, true))).ToList();
                            }

                            var attrToBeRemoved = filteredAttributes.Where(a => a.FullPath == fullPath || a.FullPath.StartsWith(fullPath)).ToList();
                            var removedRequiredAttributes = attrToBeRemoved.Where(a => a.SchemaAttribute.Required);
                            if (removedRequiredAttributes.Any())
                                throw new SCIMImmutableAttributeException(string.Format(string.Format(Global.RequiredAttributesCannotBeRemoved, string.Join(",", removedRequiredAttributes.Select(r => r.FullPath)))));

                            var removedReadOnlyAttributes = attrToBeRemoved.Where(a => a.SchemaAttribute.Mutability == SCIMSchemaAttributeMutabilities.READONLY);
                            if (removedReadOnlyAttributes.Any())
                                throw new SCIMImmutableAttributeException(string.Format(string.Format(Global.ReadOnlyAttributesCannotBeRemoved, string.Join(",", removedReadOnlyAttributes.Select(r => r.FullPath)))));

                            removeCallback(attrToBeRemoved);
                        }
                        break;
                    case SCIMPatchOperations.REPLACE:
                        {
                            UpdateCommonAttributes(patch, representation, result, schemaAttributes);
                            if(hierarchicalFilteredAttributes == null)
                            {
                                await OverrideRootAttributes(result, hierarchicalNewAttributes, representation, cancellationToken);
                            }
                            // If the target location is a multi-valued attribute and no filter is specified, the attribute and all values are replaced.
                            else if (scimFilter != null && !(scimFilter is SCIMComplexAttributeExpression) && scimExprSchemaAttr != null && scimExprSchemaAttr.MultiValued == true)
                            {
                                removeCallback(hierarchicalFilteredAttributes.Where(h => !computedIndexes.Contains(h.ComputedValueIndex)).SelectMany(a => a.ToFlat()).ToList());
                                hierarchicalNewAttributes.SelectMany(a => a.ToFlat()).ToList().ForEach(a =>
                                {
                                    a.RepresentationId = representation.Id;
                                    result.Add(a);
                                });
                            }
                            else
                            {
                                foreach (var newAttribute in hierarchicalNewAttributes)
                                {
                                    if (TryInsertWhenTargetLocationIsUnknown(result, newAttribute, hierarchicalFilteredAttributes, representation)) continue;
                                    ReplaceAttributes(result, newAttribute, hierarchicalFilteredAttributes, filteredAttributes, representation);
                                }
                            }
                        }
                        break;
                }
            }

            return result;
        }

        private List<(PatchOperationParameter, SCIMExpression)> SanitizePatchOperations(SCIMRepresentation representation, IEnumerable<PatchOperationParameter> patchLst)
        {
            int i = 0;
            var lst = patchLst.Select(p =>
            {
                SCIMAttributeExpression scimExpr = null;
                SCIMSchemaAttribute scimExprSchemaAttr = null;
                var scimFilter = SCIMFilterParser.Parse(p.Path, representation.Schemas);
                if (scimFilter != null)
                {
                    scimExpr = scimFilter as SCIMAttributeExpression;
                    scimExprSchemaAttr = scimExpr.GetLastChild().SchemaAttribute;
                }

                i++;
                return (
                    index: i, 
                    operation : p, 
                    filter : scimFilter, 
                    fullPath: p.Path, 
                    multiValued: (scimExprSchemaAttr?.MultiValued).GetValueOrDefault(),
                    type: (scimExprSchemaAttr?.Type).GetValueOrDefault());
            });
            var result = new List<(PatchOperationParameter, SCIMExpression, int)>();
            foreach(var grp in lst.GroupBy(k => k.fullPath))
            {
                var orderedElts = grp.OrderBy(i => i.index);
                var lastElt = grp.OrderByDescending(r => r.index).First();
                if (lastElt.multiValued || lastElt.filter == null)
                {
                    result.AddRange(orderedElts.Select(r => (r.operation, r.filter, r.index)));
                    continue;
                }

                if(lastElt.type == SCIMSchemaAttributeTypes.COMPLEX && orderedElts.Count() > 1)
                {
                    var mergedResult = MergePatchOperationValues(orderedElts.Select(e => e.operation));
                    lastElt.operation.Value = mergedResult;
                    result.Add((lastElt.operation, lastElt.filter, lastElt.index));
                }
                else
                {
                    result.Add((lastElt.operation, lastElt.filter, lastElt.index));
                }
            }

            return result.OrderBy(r => r.Item3).Select(r => (r.Item1, r.Item2)).ToList();
        }

        private JsonObject MergePatchOperationValues(IEnumerable<PatchOperationParameter> parameters)
        {
            var mergedResult = new JsonObject();
            foreach (var operation in parameters)
            {
                var operationValueJson = operation.Value as JsonObject;
                if (operationValueJson == null) continue;
                foreach(var kvp in operationValueJson)
                {
                    var json = kvp.Value.ToString();
                    JsonNode node = null;
                    switch(kvp.Value.GetValueKind())
                    {
                        case JsonValueKind.Object:
                        case JsonValueKind.Array:
                            node = JsonNode.Parse(json);
                            break;
                        default:
                            node = json;
                            break;
                    }

                    if (!operationValueJson.ContainsKey(kvp.Key)) mergedResult.Add(kvp.Key, node);
                    else mergedResult[kvp.Key] = node;
                }
            }

            return mergedResult;
        }

        private async Task OverrideRootAttributes(SCIMRepresentationPatchResult result, List<SCIMRepresentationAttribute> hierarchicalNewAttributes, SCIMRepresentation representation, CancellationToken cancellationToken)
        {
            foreach (var grp in hierarchicalNewAttributes.GroupBy(f => f.FullPath))
            {
                var firstAttribute = grp.First();
                var computedValueIndexLst = grp.Select(g => g.ComputedValueIndex).ToList();
                var existingAttributes = await _scimRepresentationCommandRepository.FindGraphAttributesBySchemaAttributeId(representation.Id, firstAttribute.SchemaAttributeId, cancellationToken);
                existingAttributes = SCIMRepresentation.BuildHierarchicalAttributes(existingAttributes);
                var attributesToBeRemoved = existingAttributes.Where(a => !computedValueIndexLst.Contains(a.ComputedValueIndex));
                var flatAttributesToBeRemoved = attributesToBeRemoved.SelectMany(r => r.ToFlat());
                var attributesToBeAdded = grp.Where(a => !existingAttributes.Any(ea => ea.ComputedValueIndex == a.ComputedValueIndex));
                if (!attributesToBeRemoved.Any() && !attributesToBeAdded.Any()) continue;
                foreach (var flatAttributeToBeRemoved in flatAttributesToBeRemoved) result.Remove(flatAttributeToBeRemoved);

                foreach (var newAttribute in attributesToBeAdded)
                {
                    if (newAttribute.SchemaAttribute.Type != SCIMSchemaAttributeTypes.COMPLEX)
                    {
                        newAttribute.RepresentationId = representation.Id;
                        if (attributesToBeRemoved.Any()) newAttribute.ParentAttributeId = attributesToBeRemoved.First().ParentAttributeId;
                        result.Add(newAttribute);
                    }
                    else
                    {
                        var flatAttrs = newAttribute.ToFlat();
                        foreach (var flatAttr in flatAttrs)
                        {
                            flatAttr.RepresentationId = representation.Id;
                            result.Add(flatAttr);
                        }
                    }
                }
            }
        }

        private void UpdateCommonAttributes(PatchOperationParameter patch, SCIMRepresentation representation, SCIMRepresentationPatchResult result, IEnumerable<SCIMSchemaAttribute> schemaAttributes)
        {
            bool hasExternalId = false;
            if ((hasExternalId = TryGetExternalId(patch, out string externalId)) && representation.ExternalId != externalId)
            {
                representation.ExternalId = externalId;
                result.AddExternalId();
            }

            if (!hasExternalId && (schemaAttributes == null || !schemaAttributes.Any()))
                throw new SCIMNoTargetException(string.Format(Global.AttributeIsNotRecognirzed, patch.Path));

            if (TryGetDisplayName(patch, out string displayName) && representation.DisplayName != displayName)
                representation.SetDisplayName(displayName);
        }

        private bool TryInsertComplexMultivaluedAttribute(SCIMRepresentationPatchResult result, SCIMRepresentationAttribute newAttribute, SCIMRepresentationAttribute arrayParentAttribute, SCIMSchemaAttribute scimExprSchemaAttr, SCIMRepresentation representation)
        {
            if (scimExprSchemaAttr != null && !(scimExprSchemaAttr.Type == SCIMSchemaAttributeTypes.COMPLEX && scimExprSchemaAttr.MultiValued)) return false;
            var newFlatAttributes = newAttribute.ToFlat();
            foreach (var newFlatAttr in newFlatAttributes)
            {
                newFlatAttr.RepresentationId = representation.Id;
                if (arrayParentAttribute != null && newFlatAttr.FullPath == arrayParentAttribute.FullPath) continue;
                if (arrayParentAttribute != null && newFlatAttr.GetParentFullPath() == arrayParentAttribute.FullPath) newFlatAttr.ParentAttributeId = arrayParentAttribute.Id;
                result.Add(newFlatAttr);
            }

            return true;
        }

        private bool TryInsertValueIntoArray(SCIMRepresentationPatchResult result, SCIMRepresentationAttribute newAttribute, SCIMSchemaAttribute scimExprSchemaAttr, SCIMRepresentation representation)
        {
            if (scimExprSchemaAttr != null && !(scimExprSchemaAttr.Type != SCIMSchemaAttributeTypes.COMPLEX && scimExprSchemaAttr.MultiValued)) return false;
            newAttribute.RepresentationId = representation.Id;
            result.Add(newAttribute);
            return true;
        }

        private bool TryInsertWhenTargetLocationIsUnknown(SCIMRepresentationPatchResult result, SCIMRepresentationAttribute newAttribute, List<SCIMRepresentationAttribute> hierarchicalFilteredAttributes, SCIMRepresentation representation)
        {
            if (hierarchicalFilteredAttributes.Any()) return false;
            var newFlatAttributes = newAttribute.ToFlat();
            foreach (var newFlatAttr in newFlatAttributes)
            {
                newFlatAttr.RepresentationId = representation.Id;
                result.Add(newFlatAttr);
            }

            return true;
        }

        private void ReplaceAttributes(SCIMRepresentationPatchResult result, SCIMRepresentationAttribute newAttribute, List<SCIMRepresentationAttribute> hierarchicalFilteredAttributes, List<SCIMRepresentationAttribute> flatFilteredAttributes, SCIMRepresentation representation)
        {
            var newFlatAttributes = newAttribute.ToFlat();
            foreach (var newFlatAttr in newFlatAttributes)
            {
                if (newFlatAttr.SchemaAttribute.Type == SCIMSchemaAttributeTypes.COMPLEX) continue;
                var path = newFlatAttr.FullPath;
                var schemaAttr = newFlatAttr.SchemaAttribute;
                // Update existing attributes.
                var existingAttributes = flatFilteredAttributes.Where(a => a.FullPath == path && !a.IsSimilar(newFlatAttr, true));
                foreach (var attr in existingAttributes)
                {
                    var clone = (SCIMRepresentationAttribute)newFlatAttr.Clone();
                    clone.Id = attr.Id;
                    clone.ParentAttributeId = attr.ParentAttributeId;
                    clone.RepresentationId = representation.Id;
                    result.Update(clone);
                }

                // Orphan parent.
                var orphanParents = hierarchicalFilteredAttributes.Where(a => a.CachedChildren.Any() && !a.CachedChildren.Any(c => c.FullPath == path));
                foreach (var orphanParent in orphanParents)
                {
                    var clone = (SCIMRepresentationAttribute)newFlatAttr.Clone();
                    clone.Id = Guid.NewGuid().ToString();
                    clone.ParentAttributeId = orphanParent.Id;
                    clone.RepresentationId = representation.Id;
                    result.Add(clone);
                }
            }
        }

        private static bool TryGetExternalId(PatchOperationParameter patchOperation, out string externalId) => TryGetAttributeValue(patchOperation, StandardSCIMRepresentationAttributes.ExternalId, out externalId);

        private static bool TryGetDisplayName(PatchOperationParameter patchOperation, out string displayName) => TryGetAttributeValue(patchOperation, StandardSCIMRepresentationAttributes.DisplayName, out displayName);

        private static bool TryGetAttributeValue(PatchOperationParameter patchOperation, string attrName, out string value)
        {
            value = null;
            if (patchOperation.Value == null)
            {
                return false;
            }

            var jObj = patchOperation.Value as JsonObject;
            var type = patchOperation.Value.GetValueKind();
            if (patchOperation.Path == attrName && type == JsonValueKind.String)
            {
                value = patchOperation.Value.ToString();
                return true;
            }

            if (jObj != null && jObj.ContainsKey(attrName))
            {
                value = jObj[attrName].ToString();
                return true;
            }

            return false;
        }

        private static ICollection<SCIMRepresentationAttribute> ExtractRepresentationAttributesFromJSON(ICollection<SCIMSchema> schemas, ICollection<SCIMSchemaAttribute> schemaAttributes, object obj, bool ignoreUnsupportedCanonicalValues)
        {
            var result = new List<SCIMRepresentationAttribute>();
            var jArr = obj as JsonArray;
            var jObj = obj as JsonObject;
            if (jObj != null && schemaAttributes != null &&
                schemaAttributes.Any() &&
                schemaAttributes.Count() == 1 &&
                schemaAttributes.First().Type == SCIMSchemaAttributeTypes.COMPLEX)
            {
                jArr = new JsonArray();
                jArr.Add(jObj);
            }

            var mainSchema = schemas.First(s => s.IsRootSchema);
            var extensionSchemas = schemas.Where(s => !s.IsRootSchema).ToList();
            if (jArr != null)
            {
                var schemaAttr = schemaAttributes.First();
                var schema = schemas.FirstOrDefault(s => s.HasAttribute(schemaAttr));
                result.AddRange(RepresentationHelper.BuildAttributes(jArr, schemaAttr, schema, ignoreUnsupportedCanonicalValues));
            }
            else if (jObj != null)
            {
                var resolutionResult = RepresentationHelper.Resolve(jObj, mainSchema, extensionSchemas);
                result.AddRange(RepresentationHelper.BuildRepresentationAttributes(resolutionResult, resolutionResult.AllSchemaAttributes, ignoreUnsupportedCanonicalValues, true));
            }
            else if (schemaAttributes.Any() && schemaAttributes.Count() == 1)
            {
                var schemaAttribute = schemaAttributes.First();
                switch (schemaAttribute.Type)
                {
                    case SCIMSchemaAttributeTypes.BOOLEAN:
                        var valueBoolean = false;
                        if (obj == null) throw new SCIMSchemaViolatedException(string.Format(Global.NotValidBoolean, schemaAttribute.FullPath));
                        if (!bool.TryParse(obj.ToString(), out valueBoolean)) throw new SCIMSchemaViolatedException(string.Format(Global.NotValidBoolean, schemaAttribute.FullPath));
                        result.Add(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), schemaAttribute, schemaAttribute.SchemaId, valueBoolean: valueBoolean));
                        break;
                    case SCIMSchemaAttributeTypes.STRING:
                        if (obj == null) throw new SCIMSchemaViolatedException(string.Format(Global.NotValidString, schemaAttribute.FullPath));
                        if (schemaAttribute.Required && string.IsNullOrWhiteSpace(obj.ToString())) throw new SCIMSchemaViolatedException(string.Format(Global.RequiredAttributesAreMissing, schemaAttribute.FullPath));
                        result.Add(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), schemaAttribute, schemaAttribute.SchemaId, valueString: obj.ToString()));
                        break;
                    case SCIMSchemaAttributeTypes.INTEGER:
                        int valueInteger;
                        if (obj == null) throw new SCIMSchemaViolatedException(string.Format(Global.NotValidInteger, schemaAttribute.FullPath));
                        if (!int.TryParse(obj.ToString(), out valueInteger)) throw new SCIMSchemaViolatedException(string.Format(Global.NotValidInteger, schemaAttribute.FullPath));
                        result.Add(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), schemaAttribute, schemaAttribute.SchemaId, valueInteger: valueInteger));
                        break;
                    case SCIMSchemaAttributeTypes.DATETIME:
                        DateTime dt;
                        if (obj == null) throw new SCIMSchemaViolatedException(string.Format(Global.NotValidDateTime, schemaAttribute.FullPath));
                        if (!DateTime.TryParse(obj.ToString(), out dt)) throw new SCIMSchemaViolatedException(string.Format(Global.NotValidDateTime, schemaAttribute.FullPath));
                        result.Add(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), schemaAttribute, schemaAttribute.SchemaId, valueDateTime: dt));
                        break;
                    case SCIMSchemaAttributeTypes.COMPLEX:
                        throw new SCIMSchemaViolatedException(string.Format(Global.NotValidJSON, schemaAttribute.FullPath));
                }
            }

            return result;
        }

        private static List<SCIMRepresentationAttribute> RemoveStandardReferenceProperties(IEnumerable<SCIMRepresentationAttribute> newFlatAttributes, IEnumerable<SCIMAttributeMapping> attributeMappings)
        {
            return newFlatAttributes.Where((nfa) =>
            {
                var parentAttr = newFlatAttributes.FirstOrDefault(a => a.Id == nfa.ParentAttributeId);
                if (parentAttr == null || !attributeMappings.Any(am => am.SourceAttributeId == parentAttr.SchemaAttributeId)) return true;
                if (nfa.SchemaAttribute.Name == SCIMConstants.StandardSCIMReferenceProperties.Type || nfa.SchemaAttribute.Name == SCIMConstants.StandardSCIMReferenceProperties.Display) return false;
                return true;
            }).ToList();
        }

        private static List<SCIMRepresentationAttribute> FilterDuplicate(IEnumerable<SCIMRepresentationAttribute> existingAttributes, IEnumerable<SCIMRepresentationAttribute> newHierarchicalAttributes)
        {
            var result = new List<SCIMRepresentationAttribute>();
            foreach (var newHierarchicalAttribute in newHierarchicalAttributes)
            {
                if (existingAttributes.Any(a => a.ComputedValueIndex == newHierarchicalAttribute.ComputedValueIndex)) continue;
                result.Add(newHierarchicalAttribute);
            }

            return result;
        }

        #endregion

        #region Check uniqueness

        public async Task CheckUniqueness(string realm, IEnumerable<SCIMRepresentationAttribute> attributes)
        {
            var uniqueServerAttributeIds = attributes.Where(a => a.SchemaAttribute.Uniqueness == SCIMSchemaAttributeUniqueness.SERVER);
            var uniqueGlobalAttributes = attributes.Where(a => a.SchemaAttribute.Uniqueness == SCIMSchemaAttributeUniqueness.GLOBAL);
            await CheckSCIMRepresentationExistsForGivenUniqueAttributes(realm, uniqueServerAttributeIds);
            await CheckSCIMRepresentationExistsForGivenUniqueAttributes(realm, uniqueGlobalAttributes);
        }

        private async Task CheckSCIMRepresentationExistsForGivenUniqueAttributes(string realm, IEnumerable<SCIMRepresentationAttribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                List<SCIMRepresentationAttribute> records = null;
                switch (attribute.SchemaAttribute.Type)
                {
                    case SCIMSchemaAttributeTypes.STRING:
                        records = await _scimRepresentationCommandRepository.FindAttributesByValue(attribute.SchemaAttribute.Id, attribute.ValueString);
                        break;
                    case SCIMSchemaAttributeTypes.INTEGER:
                        if (attribute.ValueInteger != null)
                        {
                            records = await _scimRepresentationCommandRepository.FindAttributesByValue(attribute.SchemaAttribute.Id, attribute.ValueInteger.Value);
                        }
                        break;
                }

                var isUnique = records != null && records.Any();
                if(!string.IsNullOrWhiteSpace(realm))
                {
                    var representations = await _scimRepresentationCommandRepository.FindRepresentations(records.Select(r => r.RepresentationId).Distinct().ToList());
                    var filteredRepresentations = representations.Where(r => r.RealmName == realm);
                    isUnique = filteredRepresentations != null && filteredRepresentations.Any();
                }

                if (isUnique)
                {
                    throw new SCIMUniquenessAttributeException(string.Format(Global.AttributeMustBeUnique, attribute.SchemaAttribute.Name));
                }
            }
        }

        #endregion

        #region Check mutability

        public void CheckMutability(List<SCIMPatchResult> patchOperations)
        {
            var rootPatchOperations = patchOperations.Where(o => !patchOperations.Any(p => o.Attr.GetParentFullPath() == p.Path));
            foreach (var rootOperation in rootPatchOperations)
            {
                var scopedOperations = new List<SCIMPatchResult>
                {
                    rootOperation
                };
                if (rootOperation.Operation == SCIMPatchOperations.ADD)
                {
                    scopedOperations  = patchOperations.Where(p => p.Path.StartsWith(rootOperation.Path) && p.Operation == rootOperation.Operation).ToList();
                }

                var attrWithBrokenMutability = scopedOperations
                    .Where(o => o.Attr != null && (o.Operation == SCIMPatchOperations.REMOVE || o.Operation == SCIMPatchOperations.REPLACE) && (o.Attr.SchemaAttribute.Mutability == SCIMSchemaAttributeMutabilities.IMMUTABLE))
                    .Select(o => o.Attr);
                if (attrWithBrokenMutability.Any())
                    throw new SCIMImmutableAttributeException(string.Format(Global.AttributeImmutable, string.Join(",", attrWithBrokenMutability.Select(a => a.FullPath).Distinct())));
            }
        }

        #endregion

        #region Extract


        public SCIMRepresentation ExtractSCIMRepresentationFromJSON(JsonObject json, string externalId, SCIMSchema mainSchema, ICollection<SCIMSchema> extensionSchemas, IEnumerable<SCIMAttributeMapping> attributeMappings)
        {
            CheckRequiredAttributes(mainSchema, extensionSchemas, json);
            return BuildRepresentation(json, externalId, mainSchema, extensionSchemas, _options.IgnoreUnsupportedCanonicalValues, attributeMappings);
        }

        public static SCIMRepresentation BuildRepresentation(JsonObject json, string externalId, SCIMSchema mainSchema, ICollection<SCIMSchema> extensionSchemas, bool ignoreUnsupportedCanonicalValues, IEnumerable<SCIMAttributeMapping> attributeMappings)
        {
            var schemas = new List<SCIMSchema>
            {
                mainSchema
            };
            schemas.AddRange(extensionSchemas);
            var result = new SCIMRepresentation
            {
                ExternalId = externalId,
                Schemas = schemas
            };
            result.Schemas = schemas;
            var resolutionResult = Resolve(json, mainSchema, extensionSchemas);
            var flatAttributes = BuildRepresentationAttributes(resolutionResult, resolutionResult.AllSchemaAttributes, ignoreUnsupportedCanonicalValues);
            flatAttributes = RemoveStandardReferenceProperties(flatAttributes, attributeMappings);
            result.FlatAttributes = flatAttributes;
            var attr = result.FlatAttributes.FirstOrDefault(a => a.SchemaAttribute.Name == "displayName");
            if (attr != null)
            {
                result.DisplayName = attr.ValueString;
            }

            return result;
        }

        public static ICollection<SCIMRepresentationAttribute> BuildRepresentationAttributes(ResolutionResult resolutionResult, ICollection<SCIMSchemaAttribute> allSchemaAttributes, bool ignoreUnsupportedCanonicalValues, bool ignoreDefaultAttrs = false)
        {
            var attributes = new List<SCIMRepresentationAttribute>();
            foreach (var record in resolutionResult.Rows)
            {
                if (record.SchemaAttribute.Mutability == SCIMSchemaAttributeMutabilities.READONLY)
                {
                    continue;
                }

                // Add attribute
                if (record.SchemaAttribute.MultiValued)
                {
                    var jArr = record.Content as JsonArray;
                    if (jArr == null && !record.Content.IsEmpty())
                    {
                        throw new SCIMSchemaViolatedException(string.Format(Global.AttributeIsNotArray, record.SchemaAttribute.Name));
                    }

                    if(jArr != null) attributes.AddRange(BuildAttributes(jArr, record.SchemaAttribute, record.Schema, ignoreUnsupportedCanonicalValues));
                }
                else
                {
                    var jArr = new JsonArray();
                    if (!record.Content.IsEmpty())
                    {
                        var clone = JsonNode.Parse(record.Content.ToJsonString());
                        jArr.Add(clone);
                    }

                    attributes.AddRange(BuildAttributes(jArr, record.SchemaAttribute, record.Schema, ignoreUnsupportedCanonicalValues));
                }
            }

            if (ignoreDefaultAttrs)
            {
                return attributes;
            }

            var defaultAttributes = allSchemaAttributes.Where(a => !attributes.Any(at => at.SchemaAttribute.Name == a.Name) && a.Mutability == SCIMSchemaAttributeMutabilities.READWRITE);
            foreach (var defaultAttr in defaultAttributes)
            {
                var attributeId = Guid.NewGuid().ToString();
                switch (defaultAttr.Type)
                {
                    case SCIMSchemaAttributeTypes.STRING:
                        if (defaultAttr.DefaultValueString.Any())
                        {
                            var defaultValueStr = defaultAttr.DefaultValueString;
                            if (!defaultAttr.MultiValued)
                            {
                                defaultValueStr = new List<string> { defaultValueStr.First() };
                            }

                            foreach (var str in defaultValueStr)
                            {
                                attributes.Add(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), attributeId, defaultAttr, defaultAttr.SchemaId, valueString: str));
                            }
                        }

                        break;
                    case SCIMSchemaAttributeTypes.INTEGER:
                        if (defaultAttr.DefaultValueInt.Any())
                        {
                            var defaultValueInt = defaultAttr.DefaultValueInt;
                            if (!defaultAttr.MultiValued)
                            {
                                defaultValueInt = new List<int> { defaultValueInt.First() };
                            }

                            foreach (var i in defaultValueInt)
                            {
                                attributes.Add(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), attributeId, defaultAttr, defaultAttr.SchemaId, valueInteger: i));
                            }
                        }

                        break;
                }
            }

            return attributes;
        }

        public static ICollection<SCIMRepresentationAttribute> BuildAttributes(JsonArray jArr, SCIMSchemaAttribute schemaAttribute, SCIMSchema schema, bool ignoreUnsupportedCanonicalValues)
        {
            var result = new List<SCIMRepresentationAttribute>();
            if (schemaAttribute.Mutability == SCIMSchemaAttributeMutabilities.READONLY)
            {
                return result;
            }

            var attributeId = Guid.NewGuid().ToString();
            if (schemaAttribute.Type == SCIMSchemaAttributeTypes.COMPLEX)
            {
                if (!jArr.Any())
                {
                    result.Add(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), attributeId, schemaAttribute, schema.Id));
                }
                else
                {
                    foreach (var jsonProperty in jArr)
                    {
                        var rec = jsonProperty as JsonObject;
                        if (rec == null)
                        {
                            throw new SCIMSchemaViolatedException(string.Format(Global.NotValidJSON, jsonProperty.ToString()));
                        }

                        var subAttributes = schema.GetChildren(schemaAttribute).ToList();
                        CheckRequiredAttributes(schema, subAttributes, rec);
                        var resolutionResult = Resolve(rec, schema, subAttributes);
                        var parent = new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), attributeId, schemaAttribute, schema.Id);
                        var children = BuildRepresentationAttributes(resolutionResult, subAttributes, ignoreUnsupportedCanonicalValues);
                        foreach (var child in children)
                        {
                            if (SCIMRepresentation.GetParentPath(child.FullPath) == parent.FullPath)
                            {
                                child.ParentAttributeId = parent.Id;
                            }

                            result.Add(child);
                        }

                        result.Add(parent);
                    }
                }
            }
            else
            {
                switch (schemaAttribute.Type)
                {
                    case SCIMSchemaAttributeTypes.BOOLEAN:
                        var valuesBooleanResult = Extract<bool>(jArr);
                        if (valuesBooleanResult.InvalidValues.Any())
                        {
                            throw new SCIMSchemaViolatedException(string.Format(Global.NotValidBoolean, string.Join(",", valuesBooleanResult.InvalidValues)));
                        }

                        foreach (var b in valuesBooleanResult.Values)
                        {
                            var record = new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), attributeId, schemaAttribute, schema.Id, valueBoolean: b);
                            result.Add(record);
                        }
                        break;
                    case SCIMSchemaAttributeTypes.INTEGER:
                        var valuesIntegerResult = Extract<int>(jArr);
                        if (valuesIntegerResult.InvalidValues.Any())
                        {
                            throw new SCIMSchemaViolatedException(string.Format(Global.NotValidInteger, string.Join(",", valuesIntegerResult.InvalidValues)));
                        }

                        foreach (var i in valuesIntegerResult.Values)
                        {
                            var record = new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), attributeId, schemaAttribute, schema.Id, valueInteger: i);
                            result.Add(record);
                        }
                        break;
                    case SCIMSchemaAttributeTypes.DATETIME:
                        var valuesDateTimeResult = Extract<DateTime>(jArr);
                        if (valuesDateTimeResult.InvalidValues.Any())
                        {
                            throw new SCIMSchemaViolatedException(string.Format(Global.NotValidDateTime, string.Join(",", valuesDateTimeResult.InvalidValues)));
                        }

                        foreach (var d in valuesDateTimeResult.Values)
                        {
                            var record = new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), attributeId, schemaAttribute, schema.Id, valueDateTime: d);
                            result.Add(record);
                        }
                        break;
                    case SCIMSchemaAttributeTypes.STRING:
                        var strs = jArr.Select(j => j.GetValueKind() == System.Text.Json.JsonValueKind.Null ? null : j.ToString()).ToList();
                        if (schemaAttribute.CanonicalValues != null
                            && schemaAttribute.CanonicalValues.Any()
                            && !ignoreUnsupportedCanonicalValues
                            && !strs.All(_ => schemaAttribute.CaseExact ?
                                schemaAttribute.CanonicalValues.Contains(_)
                                : schemaAttribute.CanonicalValues.Contains(_, StringComparer.OrdinalIgnoreCase))
                        )
                        {
                            throw new SCIMSchemaViolatedException(string.Format(Global.NotValidCanonicalValue, schemaAttribute.Name));
                        }

                        foreach (var s in strs)
                        {
                            var record = new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), attributeId, schemaAttribute, schema.Id, valueString: s);
                            result.Add(record);
                        }
                        break;
                    case SCIMSchemaAttributeTypes.REFERENCE:
                        var refs = jArr.Select(j => j.ToString()).ToList();
                        foreach (var reference in refs)
                        {
                            var record = new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), attributeId, schemaAttribute, schema.Id, valueReference: reference);
                            result.Add(record);
                        }
                        break;
                    case SCIMSchemaAttributeTypes.DECIMAL:
                        var valuesDecimalResult = Extract<decimal>(jArr);
                        if (valuesDecimalResult.InvalidValues.Any())
                        {
                            throw new SCIMSchemaViolatedException(string.Format(Global.NotValidDecimal, string.Join(",", valuesDecimalResult.InvalidValues)));
                        }

                        foreach (var d in valuesDecimalResult.Values)
                        {
                            var record = new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), attributeId, schemaAttribute, schema.Id, valueDecimal: d);
                            result.Add(record);
                        }
                        break;
                    case SCIMSchemaAttributeTypes.BINARY:
                        var invalidValues = new List<string>();
                        var valuesBinary = new List<string>();
                        foreach (var rec in jArr)
                        {
                            try
                            {
                                Convert.FromBase64String(rec.ToString());
                                valuesBinary.Add(rec.ToString());
                            }
                            catch (FormatException)
                            {
                                invalidValues.Add(rec.ToString());
                            }
                        }

                        if (invalidValues.Any())
                        {
                            throw new SCIMSchemaViolatedException(string.Format(Global.NotValidBase64, string.Join(",", invalidValues)));
                        }

                        foreach (var b in valuesBinary)
                        {
                            var record = new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), attributeId, schemaAttribute, schema.Id, valueBinary: b);
                            result.Add(record);
                        }
                        break;
                }
            }

            return result;
        }

        #endregion

        #region Resolve attributes

        public static ResolutionResult Resolve(JsonObject json, SCIMSchema mainSchema, ICollection<SCIMSchema> extensionSchemas)
        {
            var rows = new List<ResolutionRowResult>();
            var schemas = new List<SCIMSchema>
            {
                mainSchema
            };
            schemas.AddRange(extensionSchemas);
            foreach (var kvp in json)
            {
                if (kvp.Key == StandardSCIMRepresentationAttributes.Schemas || SCIMConstants.StandardSCIMCommonRepresentationAttributes.Contains(kvp.Key))
                {
                    continue;
                }

                if (extensionSchemas.Any(s => kvp.Key.StartsWith(s.Id, StringComparison.InvariantCultureIgnoreCase)))
                {
                    rows.AddRange(ResolveFullQualifiedName(kvp, extensionSchemas));
                    continue;
                }

                rows.Add(Resolve(kvp, schemas));
            }

            return new ResolutionResult(schemas, rows);
        }

        public static ResolutionResult Resolve(JsonObject json, SCIMSchema schema, ICollection<SCIMSchemaAttribute> schemaAttributes)
        {
            var rows = new List<ResolutionRowResult>();
            foreach (var kvp in json)
            {
                rows.Add(Resolve(kvp, schema, schemaAttributes));
            }

            return new ResolutionResult(rows);
        }

        private static ResolutionRowResult Resolve(KeyValuePair<string, JsonNode> kvp, ICollection<SCIMSchema> allSchemas)
        {
            var schema = allSchemas.FirstOrDefault(s => s.Attributes.Any(at => at.FullPath.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)));
            if (schema == null)
            {
                throw new SCIMSchemaViolatedException(string.Format(Global.AttributeIsNotRecognirzed, kvp.Key));
            }

            return new ResolutionRowResult(schema, schema.Attributes.First(at => at.FullPath.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)), kvp.Value);
        }

        private static ICollection<ResolutionRowResult> ResolveFullQualifiedName(KeyValuePair<string, JsonNode> kvp, ICollection<SCIMSchema> extensionSchemas)
        {
            var jObj = kvp.Value as JsonObject;
            var jArr = kvp.Value as JsonArray;
            if (jArr != null)
            {
                throw new SCIMSchemaViolatedException(string.Format(Global.PropertyCannotContainsArray, kvp.Key));
            }

            var result = new List<ResolutionRowResult>();
            if (jObj == null) return result;
            var schema = extensionSchemas.First(e => kvp.Key == e.Id);
            foreach (var skvp in jObj)
            {
                var attrSchema = schema.Attributes.FirstOrDefault(a => a.Name == skvp.Key);
                if (attrSchema == null)
                {
                    throw new SCIMSchemaViolatedException(string.Format(Global.AttributeIsNotRecognirzed, skvp.Key));
                }

                result.Add(new ResolutionRowResult(schema, attrSchema, skvp.Value));
            }

            return result;
        }

        private static ResolutionRowResult Resolve(KeyValuePair<string, JsonNode> kvp, SCIMSchema schema, ICollection<SCIMSchemaAttribute> schemaAttributes)
        {
            var attrSchema = schemaAttributes.FirstOrDefault(a => a.Name.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase));
            if (attrSchema == null)
            {
                throw new SCIMSchemaViolatedException(string.Format(Global.AttributeIsNotRecognirzed, kvp.Key));
            }

            return new ResolutionRowResult(schema, attrSchema, kvp.Value);
        }

        #endregion

        #region Check required attributes

        private static void CheckRequiredAttributes(SCIMSchema mainSchema, ICollection<SCIMSchema> extensionSchemas, JsonObject json)
        {
            CheckRequiredAttributes(mainSchema, json);
            foreach (var extensionSchema in extensionSchemas)
            {
                CheckRequiredAttributes(extensionSchema, json);
            }
        }

        private static void CheckRequiredAttributes(SCIMSchema schema, JsonObject json)
        {
            var attributes = schema.HierarchicalAttributes.Select(h => h.Leaf);
            CheckRequiredAttributes(schema, attributes, json);
        }

        private static void CheckRequiredAttributes(SCIMSchema schema, IEnumerable<SCIMSchemaAttribute> schemaAttributes, JsonObject json)
        {
            var missingRequiredAttributes = schemaAttributes.Where(a => a.Required && !json.HasElement(a.Name, schema.Id));
            if (missingRequiredAttributes.Any())
            {
                throw new SCIMSchemaViolatedException(string.Format(Global.RequiredAttributesAreMissing, string.Join(",", missingRequiredAttributes.Select(a => $"{schema.Id}:{a.FullPath}"))));
            }
        }

        #endregion

        public class ResolutionRowResult
        {
            public ResolutionRowResult(SCIMSchemaAttribute schemaAttribute, JsonNode content)
            {
                SchemaAttribute = schemaAttribute;
                Content = content;
            }

            public ResolutionRowResult(SCIMSchema schema, SCIMSchemaAttribute schemaAttribute, JsonNode content) : this(schemaAttribute, content)
            {
                Schema = schema;
            }

            public SCIMSchema Schema { get; set; }
            public SCIMSchemaAttribute SchemaAttribute { get; set; }
            public JsonNode Content { get; set; }
        }

        public class ResolutionResult
        {
            public ResolutionResult(ICollection<ResolutionRowResult> rows)
            {
                Rows = rows;
            }

            public ResolutionResult(ICollection<SCIMSchema> schemas, ICollection<ResolutionRowResult> rows) : this(rows)
            {
                Schemas = schemas;
                Rows = rows;
            }

            public ICollection<SCIMSchema> Schemas { get; set; }
            public ICollection<ResolutionRowResult> Rows { get; set; }
            public ICollection<SCIMSchemaAttribute> AllSchemaAttributes
            {
                get
                {
                    return Schemas.SelectMany(s => s.HierarchicalAttributes).Select(h => h.Leaf).ToList();
                }
            }
        }

        private class ExtractionResult<T>
        {
            public ExtractionResult()
            {
                InvalidValues = new List<string>();
                Values = new List<T>();
            }

            public ICollection<string> InvalidValues { get; set; }
            public ICollection<T> Values { get; set; }
        }

        private static ExtractionResult<T> Extract<T>(JsonArray jArr) where T : struct
        {
            var result = new ExtractionResult<T>();
            var type = typeof(T);
            Type[] argTypes = { typeof(string), type.MakeByRefType() };
            var method = typeof(T).GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, Type.DefaultBinder, argTypes, null);
            foreach (var record in jArr)
            {
                if (record.GetValueKind() == System.Text.Json.JsonValueKind.Null) continue;
                var parameters = new object[] { record.ToString(), null };
                var success = (bool)method.Invoke(null, parameters);
                if (!success)
                {
                    result.InvalidValues.Add(record.ToString());
                }
                else
                {
                    var retVal = (T)parameters[1];
                    result.Values.Add(retVal);
                }
            }

            return result;
        }
    }
}

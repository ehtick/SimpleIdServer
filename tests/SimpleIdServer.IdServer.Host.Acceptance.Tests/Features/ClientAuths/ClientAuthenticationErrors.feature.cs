﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SimpleIdServer.IdServer.Host.Acceptance.Tests.Features.ClientAuths
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class ClientAuthenticationErrorsFeature : object, Xunit.IClassFixture<ClientAuthenticationErrorsFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "ClientAuthenticationErrors.feature"
#line hidden
        
        public ClientAuthenticationErrorsFeature(ClientAuthenticationErrorsFeature.FixtureData fixtureData, SimpleIdServer_IdServer_Host_Acceptance_Tests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features/ClientAuths", "ClientAuthenticationErrors", "\tCheck common errors returned during the client authentication", ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public void TestInitialize()
        {
        }
        
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Error is returned when client_id is missing")]
        [Xunit.TraitAttribute("FeatureTitle", "ClientAuthenticationErrors")]
        [Xunit.TraitAttribute("Description", "Error is returned when client_id is missing")]
        public void ErrorIsReturnedWhenClient_IdIsMissing()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Error is returned when client_id is missing", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 4
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table73 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table73.AddRow(new string[] {
                            "grant_type",
                            "client_credentials"});
                table73.AddRow(new string[] {
                            "scope",
                            "scope"});
#line 5
 testRunner.When("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table73, "When ");
#line hidden
#line 10
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 11
 testRunner.Then("HTTP status code equals to \'400\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 12
 testRunner.And("JSON \'$.error\'=\'invalid_client\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 13
 testRunner.And("JSON \'$.error_description\'=\'missing client_id\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Error is returned when client_id is invalid")]
        [Xunit.TraitAttribute("FeatureTitle", "ClientAuthenticationErrors")]
        [Xunit.TraitAttribute("Description", "Error is returned when client_id is invalid")]
        public void ErrorIsReturnedWhenClient_IdIsInvalid()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Error is returned when client_id is invalid", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 15
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table74 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table74.AddRow(new string[] {
                            "grant_type",
                            "client_credentials"});
                table74.AddRow(new string[] {
                            "scope",
                            "scope"});
                table74.AddRow(new string[] {
                            "client_id",
                            "c"});
#line 16
 testRunner.When("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table74, "When ");
#line hidden
#line 22
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 23
 testRunner.Then("HTTP status code equals to \'400\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 24
 testRunner.And("JSON \'$.error\'=\'invalid_client\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 25
 testRunner.And("JSON \'$.error_description\'=\'unknown client c\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Error is returned when client_secret is missing")]
        [Xunit.TraitAttribute("FeatureTitle", "ClientAuthenticationErrors")]
        [Xunit.TraitAttribute("Description", "Error is returned when client_secret is missing")]
        public void ErrorIsReturnedWhenClient_SecretIsMissing()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Error is returned when client_secret is missing", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 27
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table75 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table75.AddRow(new string[] {
                            "grant_type",
                            "client_credentials"});
                table75.AddRow(new string[] {
                            "scope",
                            "scope"});
                table75.AddRow(new string[] {
                            "client_id",
                            "firstClient"});
#line 28
 testRunner.When("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table75, "When ");
#line hidden
#line 34
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 35
 testRunner.Then("HTTP status code equals to \'400\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 36
 testRunner.And("JSON \'$.error\'=\'invalid_client\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 37
 testRunner.And("JSON \'$.error_description\'=\'bad client credential\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Error is returned when client_secret is invalid")]
        [Xunit.TraitAttribute("FeatureTitle", "ClientAuthenticationErrors")]
        [Xunit.TraitAttribute("Description", "Error is returned when client_secret is invalid")]
        public void ErrorIsReturnedWhenClient_SecretIsInvalid()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Error is returned when client_secret is invalid", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 39
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table76 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table76.AddRow(new string[] {
                            "grant_type",
                            "client_credentials"});
                table76.AddRow(new string[] {
                            "scope",
                            "scope"});
                table76.AddRow(new string[] {
                            "client_id",
                            "firstClient"});
                table76.AddRow(new string[] {
                            "client_secret",
                            "bad"});
#line 40
 testRunner.When("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table76, "When ");
#line hidden
#line 47
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 48
 testRunner.Then("HTTP status code equals to \'400\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 49
 testRunner.And("JSON \'$.error\'=\'invalid_client\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 50
 testRunner.And("JSON \'$.error_description\'=\'bad client credential\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Error is returned when client_assertion_type is not supported")]
        [Xunit.TraitAttribute("FeatureTitle", "ClientAuthenticationErrors")]
        [Xunit.TraitAttribute("Description", "Error is returned when client_assertion_type is not supported")]
        public void ErrorIsReturnedWhenClient_Assertion_TypeIsNotSupported()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Error is returned when client_assertion_type is not supported", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 52
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table77 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table77.AddRow(new string[] {
                            "grant_type",
                            "client_credentials"});
                table77.AddRow(new string[] {
                            "scope",
                            "scope"});
                table77.AddRow(new string[] {
                            "client_assertion_type",
                            "invalid"});
#line 53
 testRunner.When("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table77, "When ");
#line hidden
#line 59
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 60
 testRunner.Then("HTTP status code equals to \'400\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 61
 testRunner.And("JSON \'$.error\'=\'invalid_request\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 62
 testRunner.And("JSON \'$.error_description\'=\'client assertion type invalid is not supported\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Error is returned when client_assertion_type is specified but client_assertion is" +
            " missing")]
        [Xunit.TraitAttribute("FeatureTitle", "ClientAuthenticationErrors")]
        [Xunit.TraitAttribute("Description", "Error is returned when client_assertion_type is specified but client_assertion is" +
            " missing")]
        public void ErrorIsReturnedWhenClient_Assertion_TypeIsSpecifiedButClient_AssertionIsMissing()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Error is returned when client_assertion_type is specified but client_assertion is" +
                    " missing", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 64
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table78 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table78.AddRow(new string[] {
                            "grant_type",
                            "client_credentials"});
                table78.AddRow(new string[] {
                            "scope",
                            "scope"});
                table78.AddRow(new string[] {
                            "client_assertion_type",
                            "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"});
#line 65
 testRunner.When("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table78, "When ");
#line hidden
#line 71
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 72
 testRunner.Then("HTTP status code equals to \'400\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 73
 testRunner.And("JSON \'$.error\'=\'invalid_request\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 74
 testRunner.And("JSON \'$.error_description\'=\'client assertion is missing\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Error is returned when client_assertion is invalid")]
        [Xunit.TraitAttribute("FeatureTitle", "ClientAuthenticationErrors")]
        [Xunit.TraitAttribute("Description", "Error is returned when client_assertion is invalid")]
        public void ErrorIsReturnedWhenClient_AssertionIsInvalid()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Error is returned when client_assertion is invalid", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 76
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table79 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table79.AddRow(new string[] {
                            "grant_type",
                            "client_credentials"});
                table79.AddRow(new string[] {
                            "scope",
                            "scope"});
                table79.AddRow(new string[] {
                            "client_assertion_type",
                            "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"});
                table79.AddRow(new string[] {
                            "client_assertion",
                            "invalid"});
#line 77
 testRunner.When("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table79, "When ");
#line hidden
#line 84
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 85
 testRunner.Then("HTTP status code equals to \'400\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 86
 testRunner.And("JSON \'$.error\'=\'invalid_request\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 87
 testRunner.And("JSON \'$.error_description\'=\'client_assertion is not a valid JWT token\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                ClientAuthenticationErrorsFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                ClientAuthenticationErrorsFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
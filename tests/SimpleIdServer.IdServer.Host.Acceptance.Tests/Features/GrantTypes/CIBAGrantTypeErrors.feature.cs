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
namespace SimpleIdServer.IdServer.Host.Acceptance.Tests.Features.GrantTypes
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class CIBAGrantTypeErrorsFeature : object, Xunit.IClassFixture<CIBAGrantTypeErrorsFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "CIBAGrantTypeErrors.feature"
#line hidden
        
        public CIBAGrantTypeErrorsFeature(CIBAGrantTypeErrorsFeature.FixtureData fixtureData, SimpleIdServer_IdServer_Host_Acceptance_Tests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features/GrantTypes", "CIBAGrantTypeErrors", "\tCheck errors returned when using \'urn:openid:params:grant-type:ciba\' grant-type", ProgrammingLanguage.CSharp, featureTags);
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
        
        [Xunit.SkippableFactAttribute(DisplayName="Only PING or POLL modes are supported to get a token")]
        [Xunit.TraitAttribute("FeatureTitle", "CIBAGrantTypeErrors")]
        [Xunit.TraitAttribute("Description", "Only PING or POLL modes are supported to get a token")]
        public void OnlyPINGOrPOLLModesAreSupportedToGetAToken()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Only PING or POLL modes are supported to get a token", null, tagsOfScenario, argumentsOfScenario, featureTags);
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
                TechTalk.SpecFlow.Table table166 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table166.AddRow(new string[] {
                            "grant_type",
                            "urn:openid:params:grant-type:ciba"});
                table166.AddRow(new string[] {
                            "client_id",
                            "fortyThreeClient"});
                table166.AddRow(new string[] {
                            "X-Testing-ClientCert",
                            "mtlsClient.crt"});
#line 5
 testRunner.When("execute HTTP POST request \'https://localhost:8080/mtls/token\'", ((string)(null)), table166, "When ");
#line hidden
#line 11
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 12
 testRunner.Then("HTTP status code equals to \'400\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 13
 testRunner.And("JSON \'$.error\'=\'invalid_request\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 14
 testRunner.And("JSON \'$.error_description\'=\'only ping or poll mode can be used to get tokens\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="parameter auth_req_id is required")]
        [Xunit.TraitAttribute("FeatureTitle", "CIBAGrantTypeErrors")]
        [Xunit.TraitAttribute("Description", "parameter auth_req_id is required")]
        public void ParameterAuth_Req_IdIsRequired()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("parameter auth_req_id is required", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 16
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table167 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table167.AddRow(new string[] {
                            "grant_type",
                            "urn:openid:params:grant-type:ciba"});
                table167.AddRow(new string[] {
                            "client_id",
                            "fortyTwoClient"});
                table167.AddRow(new string[] {
                            "X-Testing-ClientCert",
                            "mtlsClient.crt"});
#line 17
 testRunner.When("execute HTTP POST request \'https://localhost:8080/mtls/token\'", ((string)(null)), table167, "When ");
#line hidden
#line 23
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 24
 testRunner.Then("HTTP status code equals to \'400\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 25
 testRunner.And("JSON \'$.error\'=\'invalid_request\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 26
 testRunner.And("JSON \'$.error_description\'=\'missing parameter auth_req_id\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="authorization request must exists")]
        [Xunit.TraitAttribute("FeatureTitle", "CIBAGrantTypeErrors")]
        [Xunit.TraitAttribute("Description", "authorization request must exists")]
        public void AuthorizationRequestMustExists()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("authorization request must exists", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 28
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table168 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table168.AddRow(new string[] {
                            "grant_type",
                            "urn:openid:params:grant-type:ciba"});
                table168.AddRow(new string[] {
                            "client_id",
                            "fortyTwoClient"});
                table168.AddRow(new string[] {
                            "X-Testing-ClientCert",
                            "mtlsClient.crt"});
                table168.AddRow(new string[] {
                            "auth_req_id",
                            "id"});
#line 29
 testRunner.When("execute HTTP POST request \'https://localhost:8080/mtls/token\'", ((string)(null)), table168, "When ");
#line hidden
#line 36
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 37
 testRunner.Then("HTTP status code equals to \'400\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 38
 testRunner.And("JSON \'$.error\'=\'invalid_grant\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 39
 testRunner.And("JSON \'$.error_description\'=\'authorization request doesn\'t exist\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="client must be the same")]
        [Xunit.TraitAttribute("FeatureTitle", "CIBAGrantTypeErrors")]
        [Xunit.TraitAttribute("Description", "client must be the same")]
        public void ClientMustBeTheSame()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("client must be the same", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 41
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 42
 testRunner.Given("authenticate a user", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table169 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table169.AddRow(new string[] {
                            "response_type",
                            "id_token"});
                table169.AddRow(new string[] {
                            "client_id",
                            "fourteenClient"});
                table169.AddRow(new string[] {
                            "state",
                            "state"});
                table169.AddRow(new string[] {
                            "response_mode",
                            "query"});
                table169.AddRow(new string[] {
                            "scope",
                            "openid email role"});
                table169.AddRow(new string[] {
                            "redirect_uri",
                            "http://localhost:8080"});
                table169.AddRow(new string[] {
                            "nonce",
                            "nonce"});
#line 44
 testRunner.When("execute HTTP GET request \'http://localhost/authorization\'", ((string)(null)), table169, "When ");
#line hidden
#line 54
 testRunner.And("extract parameter \'id_token\' from redirect url", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table170 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table170.AddRow(new string[] {
                            "client_id",
                            "fortyNineClient"});
                table170.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table170.AddRow(new string[] {
                            "scope",
                            "admin calendar"});
                table170.AddRow(new string[] {
                            "login_hint",
                            "user"});
                table170.AddRow(new string[] {
                            "user_code",
                            "password"});
#line 56
 testRunner.And("execute HTTP POST request \'https://localhost:8080/bc-authorize\'", ((string)(null)), table170, "And ");
#line hidden
#line 64
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 65
 testRunner.And("extract parameter \'auth_req_id\' from JSON body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table171 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table171.AddRow(new string[] {
                            "Authorization",
                            "Bearer $id_token$"});
                table171.AddRow(new string[] {
                            "auth_req_id",
                            "$auth_req_id$"});
#line 67
 testRunner.And("execute HTTP POST JSON request \'http://localhost/bc-callback\'", ((string)(null)), table171, "And ");
#line hidden
                TechTalk.SpecFlow.Table table172 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table172.AddRow(new string[] {
                            "grant_type",
                            "urn:openid:params:grant-type:ciba"});
                table172.AddRow(new string[] {
                            "client_id",
                            "fiftyClient"});
                table172.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table172.AddRow(new string[] {
                            "auth_req_id",
                            "$auth_req_id$"});
#line 72
 testRunner.And("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table172, "And ");
#line hidden
#line 79
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 81
 testRunner.Then("JSON \'$.error\'=\'invalid_grant\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 82
 testRunner.And("JSON \'$.error_description\'=\'the client is not authorized to use the auth_req_id\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="authorization request must be confirmed")]
        [Xunit.TraitAttribute("FeatureTitle", "CIBAGrantTypeErrors")]
        [Xunit.TraitAttribute("Description", "authorization request must be confirmed")]
        public void AuthorizationRequestMustBeConfirmed()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("authorization request must be confirmed", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 84
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 85
 testRunner.Given("authenticate a user", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table173 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table173.AddRow(new string[] {
                            "response_type",
                            "id_token"});
                table173.AddRow(new string[] {
                            "client_id",
                            "fourteenClient"});
                table173.AddRow(new string[] {
                            "state",
                            "state"});
                table173.AddRow(new string[] {
                            "response_mode",
                            "query"});
                table173.AddRow(new string[] {
                            "scope",
                            "openid email role"});
                table173.AddRow(new string[] {
                            "redirect_uri",
                            "http://localhost:8080"});
                table173.AddRow(new string[] {
                            "nonce",
                            "nonce"});
#line 87
 testRunner.When("execute HTTP GET request \'http://localhost/authorization\'", ((string)(null)), table173, "When ");
#line hidden
#line 97
 testRunner.And("extract parameter \'id_token\' from redirect url", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table174 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table174.AddRow(new string[] {
                            "client_id",
                            "fortyNineClient"});
                table174.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table174.AddRow(new string[] {
                            "scope",
                            "admin calendar"});
                table174.AddRow(new string[] {
                            "login_hint",
                            "user"});
                table174.AddRow(new string[] {
                            "user_code",
                            "password"});
#line 99
 testRunner.And("execute HTTP POST request \'https://localhost:8080/bc-authorize\'", ((string)(null)), table174, "And ");
#line hidden
#line 107
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 108
 testRunner.And("extract parameter \'auth_req_id\' from JSON body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table175 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table175.AddRow(new string[] {
                            "grant_type",
                            "urn:openid:params:grant-type:ciba"});
                table175.AddRow(new string[] {
                            "client_id",
                            "fortyNineClient"});
                table175.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table175.AddRow(new string[] {
                            "auth_req_id",
                            "$auth_req_id$"});
#line 110
 testRunner.And("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table175, "And ");
#line hidden
#line 117
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 119
 testRunner.Then("JSON \'$.error\'=\'authorization_pending\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 120
 testRunner.And("JSON \'$.error_description\'=\'the authorization request has not been confirmed\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="authorization request cannot be rejected")]
        [Xunit.TraitAttribute("FeatureTitle", "CIBAGrantTypeErrors")]
        [Xunit.TraitAttribute("Description", "authorization request cannot be rejected")]
        public void AuthorizationRequestCannotBeRejected()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("authorization request cannot be rejected", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 122
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 123
 testRunner.Given("authenticate a user", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table176 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table176.AddRow(new string[] {
                            "response_type",
                            "id_token"});
                table176.AddRow(new string[] {
                            "client_id",
                            "fourteenClient"});
                table176.AddRow(new string[] {
                            "state",
                            "state"});
                table176.AddRow(new string[] {
                            "response_mode",
                            "query"});
                table176.AddRow(new string[] {
                            "scope",
                            "openid email role"});
                table176.AddRow(new string[] {
                            "redirect_uri",
                            "http://localhost:8080"});
                table176.AddRow(new string[] {
                            "nonce",
                            "nonce"});
#line 125
 testRunner.When("execute HTTP GET request \'http://localhost/authorization\'", ((string)(null)), table176, "When ");
#line hidden
#line 135
 testRunner.And("extract parameter \'id_token\' from redirect url", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table177 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table177.AddRow(new string[] {
                            "client_id",
                            "fortyNineClient"});
                table177.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table177.AddRow(new string[] {
                            "scope",
                            "admin calendar"});
                table177.AddRow(new string[] {
                            "login_hint",
                            "user"});
                table177.AddRow(new string[] {
                            "user_code",
                            "password"});
#line 137
 testRunner.And("execute HTTP POST request \'https://localhost:8080/bc-authorize\'", ((string)(null)), table177, "And ");
#line hidden
#line 145
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 146
 testRunner.And("extract parameter \'auth_req_id\' from JSON body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table178 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table178.AddRow(new string[] {
                            "Authorization",
                            "Bearer $id_token$"});
                table178.AddRow(new string[] {
                            "auth_req_id",
                            "$auth_req_id$"});
                table178.AddRow(new string[] {
                            "action",
                            "1"});
#line 148
 testRunner.And("execute HTTP POST JSON request \'http://localhost/bc-callback\'", ((string)(null)), table178, "And ");
#line hidden
                TechTalk.SpecFlow.Table table179 = new TechTalk.SpecFlow.Table(new string[] {
                            "Key",
                            "Value"});
                table179.AddRow(new string[] {
                            "grant_type",
                            "urn:openid:params:grant-type:ciba"});
                table179.AddRow(new string[] {
                            "client_id",
                            "fortyNineClient"});
                table179.AddRow(new string[] {
                            "client_secret",
                            "password"});
                table179.AddRow(new string[] {
                            "auth_req_id",
                            "$auth_req_id$"});
#line 154
 testRunner.And("execute HTTP POST request \'https://localhost:8080/token\'", ((string)(null)), table179, "And ");
#line hidden
#line 161
 testRunner.And("extract JSON from body", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 163
 testRunner.Then("JSON \'$.error\'=\'access_denied\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 164
 testRunner.And("JSON \'$.error_description\'=\'the authorization request has been rejected\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
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
                CIBAGrantTypeErrorsFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                CIBAGrantTypeErrorsFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
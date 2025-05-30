﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using FormBuilder.Link;
using FormBuilder.Models;
using FormBuilder.Models.Rules;
using FormBuilder.Models.Transformer;
using FormBuilder.Transformers;
using SimpleIdServer.IdServer.Email;
using SimpleIdServer.IdServer.Email.UI.ViewModels;
using SimpleIdServer.IdServer.UI.ViewModels;
using System.Collections.ObjectModel;

namespace FormBuilder.Builders;

public static class StandardEmailRegisterWorkflows
{
    public static string workflowId = "d53b24b4-7a8f-4dd3-8fc9-7a3888ab8d93";

    public static WorkflowRecord DefaultWorkflow = WorkflowBuilder.New(workflowId)
        .AddEmailRegistration()
        .Build(DateTime.UtcNow);

    public static WorkflowBuilder AddEmailRegistration(this WorkflowBuilder builder, FormRecord? nextStep = null)
    {
        builder.AddStep(StandardEmailRegistrationForms.EmailForm)
            .AddLinkHttpRequestAction(StandardEmailRegistrationForms.EmailForm, Constants.EmptyStep, StandardEmailRegistrationForms.emailSendConfirmationCodeFormId, "Confirmation code", new WorkflowLinkHttpRequestParameter
            {
                Method = HttpMethods.POST,
                IsAntiforgeryEnabled = true,
                Target = "/{realm}/" + SimpleIdServer.IdServer.Email.Constants.AMR + "/Register",
                Transformers = new List<ITransformerParameters>
                {
                    new RegexTransformerParameters()
                    {
                        Rules = new ObservableCollection<MappingRule>
                        {
                            new MappingRule { Source = $"$.{nameof(RegisterEmailViewModel.Realm)}", Target = "realm" }
                        }
                    },
                    new RelativeUrlTransformerParameters()
                }
            }, false)
            .AddLinkHttpRequestAction(StandardEmailRegistrationForms.EmailForm, nextStep ?? Constants.EmptyStep, StandardEmailRegistrationForms.emailRegisterFormId, "Register", new WorkflowLinkHttpRequestParameter
            {
                Method = HttpMethods.POST,
                IsAntiforgeryEnabled = true,
                Target = "/{realm}/" + SimpleIdServer.IdServer.Email.Constants.AMR + "/Register",
                Transformers = new List<ITransformerParameters>
                {
                    new RegexTransformerParameters()
                    {
                        Rules = new ObservableCollection<MappingRule>
                        {
                            new MappingRule { Source = $"$.{nameof(RegisterEmailViewModel.Realm)}", Target = "realm" }
                        }
                    },
                    new RelativeUrlTransformerParameters()
                }
            }, true)
             .AddTransformedLinkUrlAction(StandardEmailRegistrationForms.EmailForm, nextStep ?? Constants.EmptyStep, StandardEmailRegistrationForms.backButtonId, "Back", "{returnUrl}", new List<ITransformerParameters>
             {
                 new RegexTransformerParameters
                 {
                     Rules = new ObservableCollection<MappingRule>
                     {
                         new MappingRule { Source = $"$.{nameof(IRegisterViewModel.ReturnUrl)}", Target = "returnUrl" }
                     }
                 }
             }, false);
        return builder;
    }
}

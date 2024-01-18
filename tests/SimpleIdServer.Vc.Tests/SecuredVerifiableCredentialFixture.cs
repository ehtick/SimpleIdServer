﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using SimpleIdServer.Did;
using SimpleIdServer.Did.Crypto;
using SimpleIdServer.Did.Crypto.Multicodec;
using SimpleIdServer.Did.Models;
using System.Text.Json.Nodes;

namespace SimpleIdServer.Vc.Tests;

public class SecuredVerifiableCredentialFixture
{
    private const string _json = "{\r\n    \"@context\": [\r\n      \"https://www.w3.org/2018/credentials/v1\",\r\n      \"https://www.w3.org/2018/credentials/examples/v1\",\r\n      \"https://w3id.org/security/suites/jws-2020/v1\"\r\n    ],\r\n    \"id\": \"http://example.gov/credentials/3732\",\r\n    \"type\": [\"VerifiableCredential\", \"UniversityDegreeCredential\"],\r\n    \"issuer\": {\r\n      \"id\": \"https://example.com/issuer/123\"\r\n    },\r\n    \"issuanceDate\": \"2020-03-10T04:24:12.164Z\",\r\n    \"credentialSubject\": {\r\n      \"id\": \"did:example:456\",\r\n      \"degree\": {\r\n        \"type\": \"BachelorDegree\",\r\n        \"name\": \"Bachelor of Science and Arts\"\r\n      }\r\n    }\r\n}";

    #region Ed25519VerificationKey2020

    [Test]
    public void When_Secure_With_ED25519_And_Ed25519VerificationKey2020_Then_Proof_Is_AddedAndValid()
    {
        // ARRANGE
        var ed25119Sig = Ed25519SignatureKey.Generate();
        var identityDocument = DidDocumentBuilder.New("did", true)
            .AddAlsoKnownAs("didSubject")
            .AddController("didController")
            .AddEd25519VerificationKey2020VerificationMethod(ed25119Sig, "controller", VerificationMethodUsages.AUTHENTICATION)
            .Build();
        var vc = SecuredVerifiableCredential.New();

        // ACT
        var securedJson = vc.Secure(_json, identityDocument, identityDocument.VerificationMethod.First().Id);
        var isSignatureValid = vc.Check(securedJson, identityDocument);

        // ASSERT
        Assert.IsNotNull(securedJson);
        var jObj = JsonObject.Parse(securedJson).AsObject();
        Assert.True(jObj.ContainsKey("proof"));
        Assert.True(isSignatureValid);
    }

    [Test]
    public void When_Secure_WithES256K_And_Ed25519VerificationKey2020_Then_Proof_Is_AddedAndValid()
    {
        // ARRANGE
        var es256K = ES256KSignatureKey.Generate();
        var identityDocument = DidDocumentBuilder.New("did", true)
            .AddAlsoKnownAs("didSubject")
            .AddController("didController")
            .AddEd25519VerificationKey2020VerificationMethod(es256K, "controller", VerificationMethodUsages.AUTHENTICATION)
            .Build();
        var vc = SecuredVerifiableCredential.New();

        // ACT
        var securedJson = vc.Secure(_json, identityDocument, identityDocument.VerificationMethod.First().Id);
        var isSignatureValid = vc.Check(securedJson, identityDocument);

        // ASSERT
        Assert.IsNotNull(securedJson);
        var jObj = JsonObject.Parse(securedJson).AsObject();
        Assert.True(jObj.ContainsKey("proof"));
        Assert.True(isSignatureValid);
    }

    [Test]
    public void When_Secure_WithES256_And_Ed25519VerificationKey2020_Then_Proof_Is_AddedAndValid()
    {
        // ARRANGE
        var es256 = ES256SignatureKey.Generate();
        var identityDocument = DidDocumentBuilder.New("did", true)
            .AddAlsoKnownAs("didSubject")
            .AddController("didController")
            .AddEd25519VerificationKey2020VerificationMethod(es256, "controller", VerificationMethodUsages.AUTHENTICATION)
            .Build();
        var vc = SecuredVerifiableCredential.New();

        // ACT
        var securedJson = vc.Secure(_json, identityDocument, identityDocument.VerificationMethod.First().Id);
        var isSignatureValid = vc.Check(securedJson, identityDocument);

        // ASSERT
        Assert.IsNotNull(securedJson);
        var jObj = JsonObject.Parse(securedJson).AsObject();
        Assert.True(jObj.ContainsKey("proof"));
        Assert.True(isSignatureValid);
    }

    [Test]
    public void When_Secure_WithES256_And_Ed25519VerificationKey2020_And_StaticPrivateKey_Then_Proof_Is_Valid()
    {
        // Test vector : https://w3c.github.io/vc-di-eddsa/#representation-ed25519signature2020
        // ARRANGE
        string json = "{\r\n    \"@context\": [\r\n        \"https://www.w3.org/ns/credentials/v2\",\r\n        \"https://www.w3.org/ns/credentials/examples/v2\"\r\n    ],\r\n    \"id\": \"urn:uuid:58172aac-d8ba-11ed-83dd-0b3aef56cc33\",\r\n    \"type\": [\"VerifiableCredential\", \"AlumniCredential\"],\r\n    \"name\": \"Alumni Credential\",\r\n    \"description\": \"A minimum viable example of an Alumni Credential.\",\r\n    \"issuer\": \"https://vc.example/issuers/5678\",\r\n    \"validFrom\": \"2023-01-01T00:00:00Z\",\r\n    \"credentialSubject\": {\r\n        \"id\": \"did:example:abcdefgh\",\r\n        \"alumniOf\": \"The School of Examples\"\r\n    }\r\n}";
        var publicKeybase = "z6MkrJVnaZkeFzdQyMZu1cgjg7k1pZZ6pvBQ7XJPt4swbTQ2";
        var privateKeybase = "z3u2en7t5LR2WtQH5PfFqMqwVHBeXouLzo6haApm8XHqvjxq";
        var factory = MulticodecSerializerFactory.Build();
        var asymKey = factory.Deserialize(publicKeybase, privateKeybase);
        var identityDocument = DidDocumentBuilder.New("did", true)
            .AddAlsoKnownAs("didSubject")
            .AddController("didController")
            .AddEd25519VerificationKey2020VerificationMethod(asymKey, "controller", VerificationMethodUsages.AUTHENTICATION, id: "https://vc.example/issuers/5678#z6MkrJVnaZkeFzdQyMZu1cgjg7k1pZZ6pvBQ7XJPt4swbTQ2")
            .Build();
        var vc = SecuredVerifiableCredential.New();

        // ACT
        var securedJson = vc.Secure(json, identityDocument, identityDocument.VerificationMethod.First().Id, creationDateTime: DateTime.Parse("2023-02-24T23:36:38Z", null, System.Globalization.DateTimeStyles.RoundtripKind));
        var isSignatureValid = vc.Check(securedJson, identityDocument);

        // ASSERT
        Assert.IsNotNull(securedJson);
        Assert.IsTrue(isSignatureValid);
        var jObj = JsonObject.Parse(securedJson).AsObject();
        Assert.True(jObj.ContainsKey("proof"));
        var proofValue = jObj["proof"]["proofValue"].ToString();
        Assert.AreEqual("z2PC6JBDG1otY3PfnxGnvHCuh8tEqPPNpDggvsnzjr2yKxszQg5bJXsQhV1ZUTG6KBNGdvWVzVqFxtLagbdoRUjf6", proofValue);
    }

    #endregion

    #region JsonWebKey2020

    [Test]
    public void When_Secure_With_ED25519_And_JsonWebKey2020_Then_Proof_Is_AddedAndValid()
    {
        // ARRANGE
        var ed25119Sig = Ed25519SignatureKey.Generate();
        var identityDocument = DidDocumentBuilder.New("did", true)
            .AddAlsoKnownAs("didSubject")
            .AddController("didController")
            .AddJsonWebKeyVerificationMethod(ed25119Sig, "controller", VerificationMethodUsages.AUTHENTICATION)
            .Build();
        var vc = SecuredVerifiableCredential.New();

        // ACT
        var securedJson = vc.Secure(_json, identityDocument, identityDocument.VerificationMethod.First().Id);
        var isSignatureValid = vc.Check(securedJson, identityDocument);

        // ASSERT
        Assert.IsNotNull(securedJson);
        var jObj = JsonObject.Parse(securedJson).AsObject();
        Assert.True(jObj.ContainsKey("proof"));
        Assert.True(isSignatureValid);
    }

    [Test]
    public void When_Secure_With_Ed25519_And_JsonWebKey2020_And_StaticPrivateKey_Then_ProofIsValid()
    {
        // ARRANGE
        var json = "{\r\n    \"@context\": [\r\n      \"https://www.w3.org/2018/credentials/v1\",\r\n      \"https://www.w3.org/2018/credentials/examples/v1\",\r\n      \"https://w3id.org/security/suites/jws-2020/v1\"\r\n    ],\r\n    \"id\": \"http://example.gov/credentials/3732\",\r\n    \"type\": [\"VerifiableCredential\", \"UniversityDegreeCredential\"],\r\n    \"issuer\": { \"id\": \"did:example:123\" },\r\n    \"issuanceDate\": \"2020-03-10T04:24:12.164Z\",\r\n    \"credentialSubject\": {\r\n      \"id\": \"did:example:456\",\r\n      \"degree\": {\r\n        \"type\": \"BachelorDegree\",\r\n        \"name\": \"Bachelor of Science and Arts\"\r\n      }\r\n    }\r\n  }";
        var x = "CV-aGlld3nVdgnhoZK0D36Wk-9aIMlZjZOK2XhPMnkQ";
        var d = "m5N7gTItgWz6udWjuqzJsqX-vksUnxJrNjD5OilScBc";
        var xPayload = Base64UrlEncoder.DecodeBytes(x);
        var dPayload = Base64UrlEncoder.DecodeBytes(d);
        var ed25119Sig = Ed25519SignatureKey.From(xPayload, dPayload);
        var identityDocument = DidDocumentBuilder.New("did", true)
            .AddController("didController")
            .AddJsonWebKeyVerificationMethod(ed25119Sig, "controller", VerificationMethodUsages.AUTHENTICATION)
            .Build();
        var vc = SecuredVerifiableCredential.New();

        // ACT
        var securedJson = vc.Secure(_json, identityDocument, identityDocument.VerificationMethod.First().Id);

        Assert.IsNotNull(securedJson);
    }

    #endregion
}
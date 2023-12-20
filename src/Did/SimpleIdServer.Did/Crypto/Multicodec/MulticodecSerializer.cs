﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using SimpleIdServer.Did.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdServer.Did.Crypto.Multicodec;

public interface IMulticodecSerializer
{
    ISignatureKey Deserialize(byte[] payload);
    byte[] GetPublicKey(string multicodecValue);
    string Serialize(ISignatureKey signatureKey);
}

/// <summary>
/// Documentation : https://w3c-ccg.github.io/did-method-key/#signature-method-creation-algorithm
/// For the multicodec values : https://github.com/multiformats/multicodec/blob/master/table.csv
/// </summary>
public class MulticodecSerializer : IMulticodecSerializer
{
    private readonly IEnumerable<IVerificationMethod> _verificationMethods;

    public MulticodecSerializer(IEnumerable<IVerificationMethod> verificationMethods)
    {
        _verificationMethods = verificationMethods;
    }

    public ISignatureKey Deserialize(byte[] payload)
    {
        if (payload == null)throw new ArgumentNullException(nameof(payload));
        var hex = payload.ToHex(true);
        var verificationMethod = _verificationMethods.SingleOrDefault(m => hex.StartsWith(m.MulticodecHexValue));
        if (verificationMethod == null) throw new InvalidOperationException("Either the multicodec is invalid or the verification method is not supported");
        var publicKey = payload.Skip(2).ToArray();
        return verificationMethod.Build(publicKey);
    }

    public byte[] GetPublicKey(string multicodecValue)
    {
        string value = new string(multicodecValue.Skip('z').ToArray());
        var payload = Encoding.Base58Encoding.Decode(value);
        return payload.Skip(2).ToArray();
    }

    public string Serialize(ISignatureKey signatureKey)
    {
        var verificationMethod = _verificationMethods.Single(m => m.Name == signatureKey.Name);
        var publicKey = verificationMethod.MulticodecHexValue.HexToByteArray().ToList();
        publicKey.AddRange(signatureKey.GetPublicKey());
        var result = $"z{Encoding.Base58Encoding.Encode(publicKey.ToArray())}";
        return result;
    }
}
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StrawberryShake;

namespace Client
{
    [System.CodeDom.Compiler.GeneratedCode("StrawberryShake", "11.0.0")]
    public interface IChatClient
    {
        Task<IOperationResult<IGetPeople>> GetPeopleAsync(
            Optional<System.Guid> userId = default,
            CancellationToken cancellationToken = default);

        Task<IOperationResult<IGetPeople>> GetPeopleAsync(
            GetPeopleOperation operation,
            CancellationToken cancellationToken = default);

        Task<IOperationResult<ILoadChat>> LoadChatAsync(
            Optional<System.Guid> recipientId = default,
            CancellationToken cancellationToken = default);

        Task<IOperationResult<ILoadChat>> LoadChatAsync(
            LoadChatOperation operation,
            CancellationToken cancellationToken = default);

        Task<IOperationResult<ISignin>> SigninAsync(
            Optional<LoginInput> signIn = default,
            CancellationToken cancellationToken = default);

        Task<IOperationResult<ISignin>> SigninAsync(
            SigninOperation operation,
            CancellationToken cancellationToken = default);

        Task<IOperationResult<ISignup>> SignupAsync(
            Optional<CreateUserInput> newUser = default,
            CancellationToken cancellationToken = default);

        Task<IOperationResult<ISignup>> SignupAsync(
            SignupOperation operation,
            CancellationToken cancellationToken = default);
    }
}

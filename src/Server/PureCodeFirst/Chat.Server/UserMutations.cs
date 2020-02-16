using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chat.Server.DataLoader;
using Chat.Server.Repositories;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Server
{
    [ExtendObjectType(Name = "Mutation")]
    public class UserMutations
    {
        public async Task<CreateUserPayload> CreateUser(
            CreateUserInput input,
            [Service]IUserRepository userRepository,
            [Service]IPersonRepository personRepository,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(input.Name))
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("The name cannot be empty.")
                        .SetCode("USERNAME_EMPTY")
                        .Build());
            }

            if (string.IsNullOrEmpty(input.Email))
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("The email cannot be empty.")
                        .SetCode("EMAIL_EMPTY")
                        .Build());
            }

            if (string.IsNullOrEmpty(input.Password))
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("The password cannot be empty.")
                        .SetCode("PASSWORD_EMPTY")
                        .Build());
            }

            string salt = Guid.NewGuid().ToString("N");

            using var sha = SHA512.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input.Password + salt));

            Guid personId = Guid.NewGuid();

            var user = new User(
                Guid.NewGuid(),
                personId,
                input.Email,
                Convert.ToBase64String(hash),
                salt);

            var person = new Person(
                personId,
                user.Id,
                input.Name,
                input.Email,
                DateTime.UtcNow,
                input.Image,
                Array.Empty<Guid>());

            await userRepository.AddUserAsync(
                user, cancellationToken)
                .ConfigureAwait(false);

            await personRepository.AddPersonAsync(
                person, cancellationToken)
                .ConfigureAwait(false);

            return new CreateUserPayload(user, input.ClientMutationId);
        }

        public async Task<InviteFriendPayload> InviteFriendAsync(
            InviteFriendInput input,
            [GlobalState]string currentUserEmail,
            PersonByEmailDataLoader personByEmail,
            [Service]IPersonRepository personRepository,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(input.Email))
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("The email address cannot be empty.")
                        .SetCode("EMAIL_EMPTY")
                        .Build());
            }

            IReadOnlyList<Person> people =
                await personByEmail.LoadAsync(
                    cancellationToken, input.Email, currentUserEmail)
                    .ConfigureAwait(false);

            if (people[0] is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("The provided friend email address is invalid.")
                        .SetCode("EMAIL_UNKNOWN")
                        .Build());
            }

            await personRepository.AddFriendIdAsync(
                people[1].Id, people[0].Id, cancellationToken)
                .ConfigureAwait(false);

            await personRepository.AddFriendIdAsync(
                people[0].Id, people[1].Id, cancellationToken)
                .ConfigureAwait(false);

            return new InviteFriendPayload(
                people[1].AddFriendId(people[0].Id),
                input.ClientMutationId);
        }

        public async Task<LoginPayload> LoginAsync(
            LoginInput input,
            [Service]IUserRepository userRepository,
            [Service]PersonByEmailDataLoader personByEmail,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(input.Email))
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("The email mustn't be empty.")
                        .SetCode("EMAIL_EMPTY")
                        .Build());
            }

            if (string.IsNullOrEmpty(input.Password))
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("The password mustn't be empty.")
                        .SetCode("PASSWORD_EMPTY")
                        .Build());
            }

            User? user = await userRepository.GetUserAsync(
                input.Email, cancellationToken)
                .ConfigureAwait(false);

            if (user is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("The specified username or password are invalid.")
                        .SetCode("INVALID_CREDENTIALS")
                        .Build());
            }

            using var sha = SHA512.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input.Password + user.Salt));

            if (!Convert.ToBase64String(hash).Equals(user.PasswordHash, StringComparison.Ordinal))
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("The specified username or password are invalid.")
                        .SetCode("INVALID_CREDENTIALS")
                        .Build());
            }

            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Email)
            });

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Startup.SharedSecret),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            Person me = await personByEmail.LoadAsync(
                input.Email, cancellationToken)
                .ConfigureAwait(false);
            
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(token);

            return new LoginPayload(me, tokenString, "bearer", input.ClientMutationId);
        }
    }

    public class LoginInput
    {
        public LoginInput(
            string email,
            string password,
            string? clientMutationId)
        {
            Email = email;
            Password = password;
            ClientMutationId = clientMutationId;
        }

        public string Email { get; }

        public string Password { get; }

        public string? ClientMutationId { get; }
    }

    public class LoginPayload
    {
        public LoginPayload(
            Person me,
            string token,
            string scheme,
            string? clientMutationId)
        {
            Me = me;
            Token = token;
            Scheme = scheme;
            ClientMutationId = clientMutationId;
        }

        public Person Me { get; }

        public string Token { get; }

        public string Scheme { get; }

        public string? ClientMutationId { get; }
    }
}

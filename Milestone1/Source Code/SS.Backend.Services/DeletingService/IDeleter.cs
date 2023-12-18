﻿using SS.Backend.SharedNamespace;

namespace SS.Backend.Services.DeletingService
{
    public interface IDeleter
    {
        public Task<Response> DeleteAccount(string username);

    }
}

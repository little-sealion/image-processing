using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProcessing.API.Services.Interfaces;

public interface IQueuesManagement
{
    public Task<bool> SendMessage<T>(T serviceMessage, string queue, string connectionString);
}

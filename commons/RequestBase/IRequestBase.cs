using commons.Protos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace commons.RequestBase;

public interface IRequestBase : IRequest<MessageResponse>
{
    string? Validate();
}

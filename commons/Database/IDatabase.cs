using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace commons.Database;

public interface IDatabase
{
    public IDatabaseCollection<T> GetCollection<T>() where T : DatabaseModel;
}

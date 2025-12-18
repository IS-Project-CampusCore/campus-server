using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace commons.Database;

public class DatabaseException(string msg) : Exception(msg);

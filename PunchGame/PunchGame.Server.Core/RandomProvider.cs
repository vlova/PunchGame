using PunchGame.Server.Room.Core.Logic;
using System;
using System.Threading;

namespace PunchGame.Server.App
{
    public class RandomProvider : IRandomProvider
    {
        // Please, note that seed will be same for threads that created Random at same time
        // We ignore this problem because it's not huge
        ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random());

        // TODO: replace decimal to double
        public decimal GetNextChance()
        {
            return (decimal)Random.Value.NextDouble();
        }
    }
}

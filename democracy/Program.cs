using System.Threading;

namespace democracy;

class Program
{
    static void Main(string[] args)
    {

    }

    public class PaperBasedVoting()
    {
        private List<int> _submittedVotes = new();
        private readonly object voteslock = new();
        private readonly object voterslock = new();

        private List<int> _voters = new();

        private enum State
        {
            Voting,
            UnexpectedErrorHappned
        }

        public void Voting()
        {
            while (_voters.Count > 0)
            {
                if (_voters.Count > 0)
                {
                    lock (voterslock)
                    {
                        _voters.Remove(_voters.Last());
                    }
                }

                //kedves polgár 2-5 perc alatt szavaz
                Thread.Sleep(Random.Shared.Next(2000, 6000));
                var vote = Random.Shared.Next(0,4);

                //6% esély érvénytelen szavazatra
                if (Random.Shared.Next(0,100) < 6)
                {
                    vote = 4;
                }

                //5% esély random fennakadásra ami 3-10p
                if (Random.Shared.Next(0, 20) == 0)
                {
                    Thread.Sleep(Random.Shared.Next(3000, 10001));
                }

                lock (voteslock)
                {
                    _submittedVotes.Add(vote);
                }
            }
        }

        public void SimulateVoters()
        {
            Thread.Sleep(Random.Shared.Next(1000, 15001));
            var l = Enumerable.Repeat(1, Random.Shared.Next(1, 6)).ToList();
            lock (voterslock)
            {
                _voters.AddRange(l);
            }
        }
    }
}

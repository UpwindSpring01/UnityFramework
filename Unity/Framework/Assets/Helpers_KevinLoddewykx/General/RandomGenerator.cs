using UnityEngine;

namespace Helpers_KevinLoddewykx.General
{
    public static class RandomGenerator
    {
        private static Random.State _seedGenerator;
        private static int _seedGeneratorSeed = 1337;
        private static bool _seedGeneratorInitialized = false;
        public static int GenerateSeed()
        {
            // remember old seed
            var temp = Random.state;

            // initialize generator state if needed
            if (!_seedGeneratorInitialized)
            {
                Random.InitState(_seedGeneratorSeed);
                _seedGenerator = Random.state;
                _seedGeneratorInitialized = true;
            }

            // set our generator state to the seed generator
            Random.state = _seedGenerator;

            // generate our new seed
            var generatedSeed = Random.Range(int.MinValue, int.MaxValue);
            // remember the new generator state
            _seedGenerator = Random.state;
            // set the original state back so that normal random generation can continue where it left off
            Random.state = temp;

            return generatedSeed;
        }
    }
}

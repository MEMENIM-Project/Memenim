using System;
using RIS.Extensions;
using RIS.Randomizing;
using RIS.Text.Generating;

namespace Memenim.Generating
{
    public static class GeneratingManager
    {
        private static readonly string[] Smiles;

        private static string _previousSmile;



        public static readonly IUnbiasedRandom RandomGenerator;
        public static readonly IUnbiasedRandom CachedRandomGenerator;
        public static readonly StringGenerator RandomStringGenerator;



        static GeneratingManager()
        {
            Smiles = new []
            {
                "(ﾟдﾟ；)",
                "(ó﹏ò｡)",
                "(´ω｀*)",
                "(┛ಠДಠ)┛彡┻━┻",
                "(* _ω_)…",
                "(ﾉ･д･)ﾉ",
                "(⊃｡•́‿•̀｡)⊃",
                "ლ(๏‿๏ ◝ლ)",
                "ლ(*꒪ヮ꒪*)ლ",
                "(ﾉ･ｪ･)ﾉ",
                "(＾▽＾)",
                "(•‿•)",
                "(☉_☉)",
                "(,,◕ ⋏ ◕,,)",
                "(๑❛ꇳ❛๑)",
                "(-, – )…zzzZZZ",
                "┬─┬ノ( º _ ºノ)",
                "(⌒‿⌒)",
                "\\ (•◡•) /",
                "⚆ _ ⚆",
                "(づ￣ ³￣)づ",
                "ಠ‿↼"
            };

            _previousSmile = string.Empty;

            RandomGenerator = new SecureRandom();
            CachedRandomGenerator = new CachedSecureRandom(
                1 * 1024 * 1024, 1, false);
            RandomStringGenerator = new StringGenerator(
                new SecureRandom());
        }



        public static string GetRandomSmile()
        {
            var smileIndex = (int)CachedRandomGenerator
                .GetNormalizedIndex((uint)Smiles.Length);

            if (Smiles[smileIndex] != _previousSmile)
            {
                _previousSmile = Smiles[smileIndex];

                return Smiles[smileIndex];
            }

            if (smileIndex == 0)
            {
                ++smileIndex;
            }
            else if (smileIndex == Smiles.Length - 1)
            {
                --smileIndex;
            }
            else
            {
                if (Rand.Current.NextBoolean(0.5))
                    ++smileIndex;
                else
                    --smileIndex;
            }

            _previousSmile = Smiles[smileIndex];

            return Smiles[smileIndex];
        }
    }
}

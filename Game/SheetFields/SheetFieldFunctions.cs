using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Yahtzee.Game.SheetFields
{
    static class SheetFieldFunctions
    {

        public static Func<int[], int> FixedValue(int v)
            => nums => v;

        public static Func<int[], int> Sum
            = nums => nums.Sum();

        public static Func<int[], int> OfAKind(int n, Func<int[], int> returnValue)
            => nums =>
                {
                    for (int i = 0; i < 6; i++)
                        if (nums.Count(num => num == i + 1) >= n)
                            return returnValue(nums);
                    return 0;
                };

        public static Func<int[], int> Straight(int l, Func<int[], int> returnValue)
            => nums =>
                {
                    for(int i = 1; i <= 6 - (l-1); i++)
                    {
                        for(int j = 0; j < l; j++)
                        {
                            if (!nums.Contains(i + j)) break;
                            if (j == l - 1) return returnValue(nums);
                        }
                    }

                    return 0;
                };

        public static int FullHouse(int[] nums)
        {
            int[] count = new int[6];
            for (int i = 0; i < 6; i++) count[i] = nums.Count(n => n == i + 1);

            if (count.Contains(2) && count.Contains(3)) return 25;
            else return 0;
        }

        public static Func<int[], int> NumFunc(int n)
            => nums => nums.Where(i => i == n).Sum();

        public static Func<int[], int> TotalOfRange(List<SheetField> fields, bool onlyLocked, int start, int count)
            => nums => (from f in fields.GetRange(start, count) where onlyLocked ? f.Locked : true select f.Value).ToArray().Sum();

        public static Func<int[], int> TotalOfFields(List<SheetField> fields, bool onlyLocked, params int[] indices)
            => nums =>
                {
                    int total = 0;
                    foreach(int i in indices)
                        total += onlyLocked ? (fields[i].Locked ? fields[i].Value : 0) : fields[i].Value;
                    return total;
                };

        public static Func<int[], int> Threshold(Func<int[], int> func, int threshold, Func<int[], int> returnValue)
            => nums => func(nums) >= threshold ? returnValue(nums) : 0;
        
    }
}

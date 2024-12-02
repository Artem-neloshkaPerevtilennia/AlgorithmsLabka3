using System.Collections.Generic;

namespace WpfApplication1
{
	public static class Algorithm
	{
		public static int BinarySearch(List<int> keys, int target)
		{
			int left = 0;
			int right = keys.Count - 1;

			while (left <= right)
			{
				int mid = left + (right - left) / 2;

				if (keys[mid] == target)
					return mid;
				
				if (keys[mid] < target)
					left = mid + 1;
				
				else
					right = mid - 1;
			}

			return -(left + 1);
		}
	}
}
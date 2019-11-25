﻿namespace QuarklessContexts.Models
{
	public struct XRange
	{
		public int Min;
		public int Max;
		public XRange(int min, int max)
		{
			this.Min = min;
			this.Max = max;
		}
	}
}
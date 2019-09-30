using CommonTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GrainInterfaces
{
	public interface IWordInfoGrain : Orleans.IGrainWithStringKey
	{
		Task<WordInfo> GetWordInfo();
	}
}

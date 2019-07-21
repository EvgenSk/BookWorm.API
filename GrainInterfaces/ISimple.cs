using System.Threading.Tasks;

namespace OrleansSimple
{
	public interface ISimple : Orleans.IGrainWithIntegerKey
	{
		Task<string> SayHello(string greeting);
	}
}
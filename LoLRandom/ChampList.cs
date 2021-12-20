using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLRandom
{
    public class ChampList
    {
        public List<Champion> champions = new List<Champion>();

        public List<Champion> topChampions = new List<Champion>();
        public List<Champion> jungleChampions = new List<Champion>();
        public List<Champion> midChampions = new List<Champion>();
        public List<Champion> botChampions = new List<Champion>();
        public List<Champion> supportChampions = new List<Champion>();
    }
}

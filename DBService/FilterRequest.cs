using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    [Serializable]
    public class FilterRequest
    {
        // number of elements to skip
        public int Offset { get; set; }
        // number of elements to retrieve
        public int Length { get; set; }
        // string representation of search theme ids "1,2,3,4..."
        public string ThemeIds { get; set; }
        // string for filtering by crossword name, if empty - "%"
        public string CrosswordName { get; set; }
        // uid for filtering user's crosswords, if searching all uid = -1
        public long Uid { get; set; }

        public FilterRequest(int offset, int length, string themeIds, string crosswordName, long uid)
        {
            Offset = offset;
            Length = length;
            ThemeIds = themeIds;
            CrosswordName = crosswordName;
            Uid = uid;
        }
    }
}

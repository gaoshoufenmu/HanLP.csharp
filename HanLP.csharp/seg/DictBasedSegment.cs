using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.seg
{
    public abstract class DictBasedSegment : Segment
    {
        public Segment EnablePartOfSpeechTagging(bool enable)
        {
            config.natureTagging = enable;
            return this;
        }
    }
}

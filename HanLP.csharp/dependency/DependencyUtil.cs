using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.corpus.tag;

namespace HanLP.csharp.dependency
{
    public class DependencyUtil
    {
        public static String compilePOS(Nature nature)
        {
            String label = nature.ToString();
            switch (nature)
            {
                case Nature.bg:
                    label = "b";
                    break;
                case Nature.mg:
                    label = "Mg";
                    break;
                case Nature.nx:
                    label = "x";
                    break;
                case Nature.qg:
                    label = "q";
                    break;
                case Nature.ud:
                    label = "u";
                    break;
                case Nature.uj:
                    label = "u";
                    break;
                case Nature.uz:
                    label = "uzhe";
                    break;
                case Nature.ug:
                    label = "uguo";
                    break;
                case Nature.ul:
                    label = "ulian";
                    break;
                case Nature.uv:
                    label = "u";
                    break;
                case Nature.yg:
                    label = "y";
                    break;
                case Nature.zg:
                    label = "z";
                    break;
                case Nature.ntc:
                case Nature.ntcf:
                case Nature.ntcb:
                case Nature.ntch:
                case Nature.nto:
                case Nature.ntu:
                case Nature.nts:
                case Nature.nth:
                    label = "nt";
                    break;
                case Nature.nh:
                case Nature.nhm:
                case Nature.nhd:
                    label = "nz";
                    break;
                case Nature.nn:
                    label = "n";
                    break;
                case Nature.nnt:
                    label = "n";
                    break;
                case Nature.nnd:
                    label = "n";
                    break;
                case Nature.nf:
                    label = "n";
                    break;
                case Nature.ni:
                case Nature.nit:
                case Nature.nic:
                    label = "nt";
                    break;
                case Nature.nis:
                    label = "n";
                    break;
                case Nature.nm:
                    label = "n";
                    break;
                case Nature.nmc:
                    label = "nz";
                    break;
                case Nature.nb:
                    label = "nz";
                    break;
                case Nature.nba:
                    label = "nz";
                    break;
                case Nature.nbc:
                case Nature.nbp:
                case Nature.nz:
                    label = "nz";
                    break;
                case Nature.g:
                    label = "nz";
                    break;
                case Nature.gm:
                case Nature.gp:
                case Nature.gc:
                case Nature.gb:
                case Nature.gbc:
                case Nature.gg:
                case Nature.gi:
                    label = "nz";
                    break;
                case Nature.j:
                    label = "nz";
                    break;
                case Nature.i:
                    label = "nz";
                    break;
                case Nature.l:
                    label = "nz";
                    break;
                case Nature.rg:
                case Nature.Rg:
                    label = "Rg";
                    break;
                case Nature.udh:
                    label = "u";
                    break;
                case Nature.e:
                    label = "y";
                    break;
                case Nature.xx:
                    label = "x";
                    break;
                case Nature.xu:
                    label = "x";
                    break;
                case Nature.w:
                case Nature.wkz:
                case Nature.wky:
                case Nature.wyz:
                case Nature.wyy:
                case Nature.wj:
                case Nature.ww:
                case Nature.wt:
                case Nature.wd:
                case Nature.wf:
                case Nature.wn:
                case Nature.wm:
                case Nature.ws:
                case Nature.wp:
                case Nature.wb:
                case Nature.wh:
                    label = "x";
                    break;
                case Nature.begin:
                    label = "root";
                    break;
                default:
                    break;
            }

            return label;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Services.Summary
{
    public class WorkingSet
    {
        public void Prune()
        {
            // Meta: Issues of representation...can lift?

            // -                return new StringEval(dateFormatter.Format(dt));
            // +                return new StringEval(dateFormatter.Format(dt, CultureInfo.CurrentCulture));

            // Perfect inline should merge lines and highlight inner difference.

            // https://github.com/db42/FBReader/commit/0997bc8433eba662d23d661b66b9d0b86c658202
            // Delete of whole file should just be line with interface/class.
            // Skip .csproj entry


            // Repeat until converge:
            //    Sample 100 commits
            //    Categorize changes.
            //    Determine worthy pattern.

            // Patterns => What patterns?
            // * Same Line Frequently Changed Across Many Files => {Line,Files} => Render {Line, y Files ... x more}

            // * Similar Line Frequently Changed Across Many Files => {Line, Files} => Render
            //      line cluster with cross-similarity > delta

            // * Before/After Line Whitespace Changed => Drop

            // * Using => Drop

            // * Assert Statement(s) => render {Add Tests (Examples..)}

            // * Changes to .csprojs => render Project Configuration (Added Files/Removed Files/Changed Properties)

            // * Extract Method Pattern 
            //      Rosalyn..etc
 
            // Until no choice to meet max size:

            // Cross-similiarity => ??
            //     crossSim[line].Sum() / lines. (distribution?)
            // Salient lines => Rank?
        }
    }
}

using System.Collections.Generic;

namespace bakalarka5.Core.Curation;

public class DocumentAligner
{
    public List<LineAlignment> AlignLines(CurationDocument a, CurationDocument b)
    {
        var result = new List<LineAlignment>();

        int i = 0, j = 0;

        while (i < a.Lines.Count || j < b.Lines.Count)
        {
            if (i >= a.Lines.Count)
            {
                result.Add(new LineAlignment { A = null, B = b.Lines[j++], Kind = AlignmentKind.OnlyInB });
                continue;
            }

            if (j >= b.Lines.Count)
            {
                result.Add(new LineAlignment { A = a.Lines[i++], B = null, Kind = AlignmentKind.OnlyInA });
                continue;
            }

            var lineA = a.Lines[i];
            var lineB = b.Lines[j];

            if (lineA.PlainText == lineB.PlainText)
            {
                result.Add(new LineAlignment { A = lineA, B = lineB, Kind = AlignmentKind.Same });
                i++; j++;
            }
            else
            {
                // naive fallback
                result.Add(new LineAlignment { A = lineA, B = lineB, Kind = AlignmentKind.Different });
                i++; j++;
            }
        }

        return result;
    }
}
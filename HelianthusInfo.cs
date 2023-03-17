using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace Helianthus
{
  public class HelianthusInfo : GH_AssemblyInfo
  {
    public override string Name => "Helianthus";

    //Return a 24x24 pixel bitmap to represent this GHA library.
    public override Bitmap Icon => null;

    //Return a short string describing the purpose of this GHA library.
    public override string Description => "Crop-Centric Sunlight-Informed " +
            "Tool for efficient urban agriculture design and urban planning";

    public override Guid Id => new Guid("3a52b9ba-646b-4b7a-a2a7-d6da1d0b53b6");

    //Return a string identifying you or your company.
    public override string AuthorName => "";

    //Return a string representing your preferred contact details.
    public override string AuthorContact => "";
  }
}

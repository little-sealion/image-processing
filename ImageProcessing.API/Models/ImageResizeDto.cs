using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProcessing.API.Models;
public class ImageResizeDto
{
    public string FileName { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Url { get; set; }
    public string ImageContainer { get; set; }
}

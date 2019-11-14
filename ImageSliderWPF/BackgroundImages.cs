using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace ImageSliderWPF
{
    //[Serializable()]
    [Serializable, XmlRoot("BackgroundImages")]
    public class BackgroundImages
    {

        [XmlIgnore]
        public List<BitmapImage> images { get; set; }


        [XmlElement("images")]
        public byte[][] imagesByte
        {
            get
            { // serialize                
                if (images == null) return null;

                JpegBitmapEncoder encoder;
                List<byte[]> byteList = new List<byte[]>();

                foreach (var item in images)
                {
                    try
                    {
                        encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(item));
                        using (var ms = new MemoryStream())
                        {
                            encoder.Save(ms);
                            byteList.Add(ms.ToArray());
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                return byteList.ToArray();
            }
            set
            { // deserialize
                if (value == null)
                {
                    images = null;
                }
                else
                {
                    try
                    {
                        BitmapImage image;
                        images = new List<BitmapImage>();
                        foreach (byte[] item in value)
                        {
                            using (MemoryStream ms = new MemoryStream(item))
                            {
                                ms.Position = 0;

                                image = new BitmapImage();
                                image.BeginInit();
                                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                                image.CacheOption = BitmapCacheOption.OnLoad;
                                image.UriSource = null;
                                image.StreamSource = ms;
                                image.EndInit();
                                image.Freeze();

                                this.images.Add(image);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
        }

        public BackgroundImages(List<BitmapImage> Image)
        {
            this.images = Image;
        }

        public BackgroundImages()
        {
        }

    }
}

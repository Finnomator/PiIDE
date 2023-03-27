using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace PiIDE.Assets.Icons {
    public static class Icons {
        public const string IconsPath = "/Assets/Icons/";

        public static BitmapImage FileIconImg { get; } = new(new(IconsPath + "FileIcon.png", UriKind.Relative));
        private static Dictionary<string, BitmapImage> CachedImages = new();

        public static BitmapSource GetFileIcon(string filePath, bool preferCustomIcons = true) {

            if (preferCustomIcons) {
                string extension = Path.GetExtension(filePath);

                if (extension.Length <= 1)
                    return FileIconImg;

                extension = extension[1..];

                string iconPath = $"{IconsPath}IconsByFileExtension/{extension}.png";
                
                if (Path.Exists(iconPath[1..])) {
                    if (CachedImages.ContainsKey(extension))
                        return CachedImages[extension];

                    BitmapImage icon = new(new(iconPath, UriKind.Relative));
                    CachedImages[extension] = icon;
                    return icon;
                }
            }

            return GetAssociatedIcon(filePath);
        }

        public static BitmapSource GetAssociatedIcon(string filePath) {
            Icon? fileIcon = Icon.ExtractAssociatedIcon(filePath);

            if (fileIcon == null)
                return FileIconImg;

            return Imaging.CreateBitmapSourceFromHIcon(fileIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}

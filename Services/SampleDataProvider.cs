using SlideDock.Models;
using System.Collections.Generic;

namespace SlideDock.Services
{
    public class SampleDataProvider : ISampleDataProvider
    {
        public DockConfiguration GetSampleConfiguration()
        {
            return new DockConfiguration
            {
                MenuGroups = new List<MenuGroupData>
                {
                    new MenuGroupData
                    {
                        Name = "Desenvolvimento",
                        IsExpanded = true,
                        AppIcons = new List<AppIconData>
                        {
                            new AppIconData
                            {
                                Name = "Visual Studio Code",
                                ExecutablePath = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Visual Studio 2022.lnk"
                            },
                            new AppIconData
                            {
                                Name = "Notepad++",
                                ExecutablePath = @"C:\Program Files\Notepad++\notepad++.exe"
                            }
                        }
                    },
                    new MenuGroupData
                    {
                        Name = "Utilitários",
                        IsExpanded = false,
                        AppIcons = new List<AppIconData>
                        {
                            new AppIconData
                            {
                                Name = "Calculadora",
                                ExecutablePath = @"C:\Windows\System32\calc.exe"
                            },
                            new AppIconData
                            {
                                Name = "Paint",
                                ExecutablePath = @"C:\Windows\System32\mspaint.exe"
                            }
                        }
                    }
                }
            };
        }
    }
}
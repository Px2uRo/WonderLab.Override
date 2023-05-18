﻿using MinecraftLaunch.Modules.Models.Download;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wonderlab.Class.Enum;
using wonderlab.Class.Models;

namespace wonderlab.ViewModels.Pages
{
    public class WebConfigPageViewModel : ReactiveObject {
        public WebConfigPageViewModel() {
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {       
            if (e.PropertyName is nameof(DownloadCount)) {
                App.LauncherData.DownloadCount = DownloadCount;
            }
        }

        [Reactive]
        public ObservableCollection<WebConnectionTestModel> TestList { get; set; } = new();

        [Reactive]
        public bool TestListVisible { get; set; } = false;


        [Reactive]
        public int DownloadCount { get; set; } = App.LauncherData.DownloadCount;

        public void RunConnectionTestAction() {
            TestList.Clear();
            TestListVisible = true;

            //下载源
            TestList.Add(new WebConnectionTestModel("https://bmclapi2.bangbang93.com"));
            TestList.Add(new WebConnectionTestModel("https://download.mcbbs.net"));
            TestList.Add(new WebConnectionTestModel("http://launchermeta.mojang.com"));

            //皮肤
            TestList.Add(new WebConnectionTestModel("http://textures.minecraft.net"));
            TestList.Add(new WebConnectionTestModel("https://sessionserver.mojang.com"));
            TestList.Add(new WebConnectionTestModel("https://www.minecraft.net", "minecraft.net"));

            //更新服务
            TestList.Add(new WebConnectionTestModel("http://43.136.86.16:8888/", "update.wonderapi.com"));
        }

        public void SelectLsaacAction() {
            App.LauncherData.IssuingBranch = IssuingBranch.Lsaac;
        }

        public void SelectAlbertAction() {
            App.LauncherData.IssuingBranch = IssuingBranch.Albert;
        }

        public void SelectOfficialAction() {
            App.LauncherData.CurrentDownloadAPI = APIManager.Mojang;
            APIManager.Current = APIManager.Mojang;
        }

        public void SelectMcbbsAction() {
            App.LauncherData.CurrentDownloadAPI = APIManager.Mcbbs;
            APIManager.Current = APIManager.Mcbbs;
        }

        public void SelectBmclAction() {
            App.LauncherData.CurrentDownloadAPI = APIManager.Bmcl;
            APIManager.Current = APIManager.Bmcl;
        }
    }
}
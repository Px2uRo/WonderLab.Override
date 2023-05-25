﻿using Avalonia.Media;
using DynamicData;
using MinecraftLaunch.Modules.Toolkits;
using Natsurainko.Toolkits.Network;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wonderlab.Class.Models;
using wonderlab.Class.Utils;
using wonderlab.Class.ViewData;
using wonderlab.Views.Pages;

namespace wonderlab.ViewModels.Pages {
    public class ServerFindPageViewModel : ReactiveObject {
        private const string WonderApi = "http://43.136.86.16:14514/api/ServerInfo";

        public ObservableCollection<WonderServerViewData> Servers { get; set; } = new();

        public async void GetServerListAction() {
            await GetServerListAsync();
        }

        public void ReturnAction() {
            new ActionCenterPage().Navigation();
        }

        public async ValueTask GetServerListAsync() {
            try {
                Servers.Clear();

                var json = await HttpWrapper.HttpClient.GetStringAsync(WonderApi);
                var viewDatas = json.ToJsonEntity<IEnumerable<WonderServerModel>>().Select(x => x.CreateViewData<WonderServerModel, WonderServerViewData>()).ToList();
                Servers.AddRange(viewDatas);

                foreach (var x in viewDatas.AsParallel()) {
                    await x.GetServerInfoAction();
                    $"来自 {x.Data.Author} 的服务器延迟为 {x.ServerInfo.Latency}ms".ShowLog();
                }
            }
            catch (Exception ex) {
                ex.ShowLog();
            }
        }
    }
}
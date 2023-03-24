﻿using Avalonia;
using MinecaftOAuth.Authenticator;
using MinecaftOAuth.Module.Base;
using MinecaftOAuth.Module.Enum;
using MinecraftLaunch.Modules.Models.Auth;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wonderlab.Class.Utils;

namespace wonderlab.ViewModels.Dialogs
{
    public class UserAuthDialogViewModel : ReactiveObject {
        public UserAuthDialogViewModel() {
            PropertyChanged += OnPropertyChanged;
        }

        internal string Code { get; set; }

        [Reactive]
        public int CurrentAuthenticatorType { get; set; }

        [Reactive]
        public string Email { set; get; }

        [Reactive]
        public string Password { set; get; }

        [Reactive]
        public string Url { set; get; }

        [Reactive]
        public string MicrosoftTip { set; get; }

        [Reactive]
        public string MicrosoftTip1 { set; get; }

        [Reactive]
        public string AuthDialogTitle { set; get; }

        [Reactive]
        public bool IsEmailVisible { set; get; } = true;

        [Reactive]
        public bool IsPasswordVisible { set; get; } = false;

        [Reactive]
        public bool IsUrlVisible { set; get; } = false;

        [Reactive]
        public bool IsMicrosoftAuth { set; get; } = false;

        [Reactive]
        public ObservableCollection<YggdrasilAccount> YggdrasilAccounts { set; get; } = new();

        [Reactive]
        public YggdrasilAccount CurrentYggdrasilAccount { set; get; } = null;

        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(CurrentAuthenticatorType)) {
                IsEmailVisible = false;
                IsPasswordVisible = false;
                IsUrlVisible = false;
                IsMicrosoftAuth = false;

                switch (CurrentAuthenticatorType)
                {
                    case 0:
                        AuthDialogTitle = "离线验证";
                        IsEmailVisible = true;
                        break;
                    case 1:
                        AuthDialogTitle = "外置验证";
                        IsEmailVisible = true;
                        IsPasswordVisible = true;
                        IsUrlVisible = true;
                        break;
                    case 2:
                        AuthDialogTitle = "微软验证";
                        IsMicrosoftAuth = true;
                        UserAuthAction();
                        break;
                }
            }
        }

        public async void UserAuthAction() {
            Account accountData = null!;

            if (CurrentAuthenticatorType == 0) {
                OfflineAuthenticator offline = new(Email);
                accountData = offline.Auth();

                await GameAccountUtils.SaveUserDataAsync(new()
                {
                    UserToken = accountData.AccessToken,
                    UserName = accountData.Name,
                    Uuid = accountData.Uuid.ToString()
                });
            }
            else if (CurrentAuthenticatorType == 1) {
                YggdrasilAuthenticator authenticator = new(Url, Email, Password);
                var result = await authenticator.AuthAsync();

                YggdrasilAccounts = result.ToObservableCollection();
                MainWindow.Instance.Auth.AuthDialog.HideDialog();
                MainWindow.Instance.Auth.YggdrasilAccountSelector.ShowDialog();
                return;
            }
            else if (CurrentAuthenticatorType == 2) {
                MicrosoftAuthenticator authenticator = new(AuthType.Access) {               
                    ClientId = "9fd44410-8ed7-4eb3-a160-9f1cc62c824c"
                };

                var codeInfo = await authenticator.GetDeviceInfo();
                Code = codeInfo.UserCode;
                MicrosoftTip = $"使用一次性验证代码 {codeInfo.UserCode} 登录您的账户";
                MicrosoftTip1 = $"请使用浏览器访问 {codeInfo.VerificationUrl} 并输入代码 {codeInfo.UserCode} 以完成登录";

                Process.Start(new ProcessStartInfo(codeInfo.VerificationUrl) {               
                    UseShellExecute = true,
                    Verb = "open"
                });

                try {               
                    var token = await authenticator.GetTokenResponse(codeInfo);

                    accountData = await authenticator.AuthAsync(x => {
                        Trace.WriteLine($"[信息] {x}");
                    });
                    
                    await GameAccountUtils.SaveUserDataAsync(new()
                    {
                        AccessToken = ((MicrosoftAccount)accountData).RefreshToken!,
                        UserName = accountData.Name,
                        UserToken = accountData.AccessToken,
                        Uuid = accountData.Uuid.ToString(),
                        UserType = accountData.Type
                    });
                }
                catch (Exception) {
                    "草，这辣鸡启动器又又炸了".ShowMessage("我日，炸了");
                }
            }

            ResettingAction();
            $"账户 {accountData.Name} 已成功添加至启动器！欢迎回来，{accountData.Name}！".ShowMessage("成功");
        }

        public async void CopyCodeAction() {
            await Application.Current!.Clipboard!.SetTextAsync(Code);
            $"已成功将代码 {Code} 复制到剪贴板".ShowMessage("信息");
        }

        public async void SaveYggdrasilAccountAction() {
            await GameAccountUtils.SaveUserDataAsync(new() {           
                AccessToken = CurrentYggdrasilAccount.ClientToken!,
                UserName = CurrentYggdrasilAccount.Name,
                UserToken = CurrentYggdrasilAccount.AccessToken,
                Uuid = CurrentYggdrasilAccount.Uuid.ToString(),
                UserType = CurrentYggdrasilAccount.Type,
                Email = CurrentYggdrasilAccount.Email,
                Password = CurrentYggdrasilAccount.Password,
                YggdrasilUrl = CurrentYggdrasilAccount.YggdrasilServerUrl
            });

            MainWindow.Instance.Auth.YggdrasilAccountSelector.HideDialog();
            ResettingAction();
        }

        public void ResettingAction() {
            Email = string.Empty;
            Password = string.Empty;
            Url = string.Empty;
            MicrosoftTip = string.Empty;
            MicrosoftTip1 = string.Empty;
            YggdrasilAccounts = new();
            CurrentYggdrasilAccount = null!;
        }
    }
}

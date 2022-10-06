# Unity Open App Detail Settings 
## Instal Package via UPM

Add this line to UPM Window for installing this plugins

    https://github.com/3BMonsterStudio/unity-native-open-app-setting.git#{release_version}

## How To Use
Call this function for open detail app setting activity

    OpenAppSettingsSDK.OpenAppSettingDetailPopup();
# Unity Monster Mobile SDK
Bộ tool hỗ trợ quản lý show quảng cáo, log event analytics và IAP, cùng một số công cụ nhỏ liên quan
## Requirement
Cài đặt trước các package sau:
- External Dependency Manager
- [FirebaseSDK](https://developers.google.com/unity/archive) : Analytics, Crashlytics , DynamicLink, Messaging RemoteConfig
- [FacebookSDK](https://developers.facebook.com/docs/unity/downloads/)
- [AppsflyerSDK](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/releases)
- [IronsourceSDK](https://developers.is.com/ironsource-mobile/unity/unity-plugin/) hoặc [MaxApplovingSDK](https://dash.applovin.com/documentation/mediation/unity/getting-started/integration)
- In App Purchasing cài qua package manager
## Cách cài đặt
Game nên có 1 scene đầu để làm scene loading kéo dài khoảng 3- 5s để init xong các SDK cần thiết:

- B1: Kéo prefabs AdsManager vào scene Loading

![Kéo prefabs AdsManager vào scene Loading](./_Images/Step1.PNG?raw=true " B1: Kéo prefabs AdsManager vào scene Loading")

B2: Điều chỉnh lại AasManager object trong inspector window. Trong baseadsList nếu trong code implement sdk của mạng nào thì chỉ kéo mạng đó vào trong list này. VD game chỉ dùng Ironsource thì baseadsList có 1 phần tử là Ironsource

![Điều chỉnh lại baseadsList với mạng tương ứng](./_Images/Step2.PNG?raw=true "B2: Điều chỉnh lại baseadsList với mạng tương ứng")

B3: Thêm Define Symbol tương ứng với mạng qc trong Project Setting. Nếu dùng MAX thì thêm HAS_MAX_APPLOVIN. Nếu dùng Ironsource thì thêm HAS_IRONSOURCE.

30/6/2022: update thêm mediation GG ads Manager với define HAS_ADS_MANAGER (cần cài sdk admob và các network riêng). Unit ID của GAM có định dạng /22719604926/Jigsaw/RV_Mediation

![Điều chỉnh lại baseadsList với mạng tương ứng](./_Images/Step3.PNG?raw=true "B2: Điều chỉnh lại baseadsList với mạng tương ứng")

Ironsource - AdMob,Meta,AdColony,Vungle,Unity
Max - AppLovin: Mintegral,Vungle,AdMob,Google Ad Manager,Meta,Unity
GAM - Vungle, meta, unity, applovin, IS, adcolony

B4: Hoàn thành bước setup trong game khi game loading xong thì gọi các nội hàm và sử dụng. Các hàm trong SDK đều có cơ chế kiểm tra khởi tạo có đúng không và có thể sử dụng an toàn ở bất kỳ đâu. Nhưng nên sử dụng sau khi SDK của các bên và Monster SDK khởi tạo xong để dùng đúng chức năng.

## Cài đặt Id và config cho SDK

Điền Facebook ID: Trên thanh toolbar vào Facebook -> EditSetting điền id facebook vào mục Facebook App Id ở inspector.

![Điền id facebook vào mục Facebook App Id ở inspector](./_Images/Step4_FBSDK.PNG?raw=true "Điền id facebook vào mục Facebook App Id ở inspector")

Vào thư mục MonsterMobileSDK trong assets vào ScriptableObject SDKDataIdSetting chỉnh lại các id và api key theo yêu cầu của game.

![Chỉnh lại các id và api key theo yêu cầu của game](./_Images/Step5_IDsetting.PNG?raw=true "Chỉnh lại các id và api key theo yêu cầu của game")

Lưu ý trường hợp sử dụng Max Apploving thì ta cần làm thêm một bước paste admob app id vào cửa sổ intergration manager của MAX SDK.

![Paste admob app id vào cửa sổ intergration manager của MAX SDK](./_Images/Step6_settingMAX.PNG?raw=true "Paste admob app id vào cửa sổ intergration manager của MAX SDK")

Trong Inspector ScriptableObject SDKDataIdSetting, vào sửa lại script SDKDataAssets bằng cách thêm các key tương ứng vào enum InterstitialPositionType và enum RewardedPositionType, nhớ gắn đúng giá trị int cho InterPositionType để setup trên dashboard và phục vụ việc tracking.

![Sửa lại script SDKDataAssets bằng cách thêm các key tương ứng](./_Images/Step7_settingPositionType.PNG?raw=true "Sửa lại script SDKDataAssets bằng cách thêm các key tương ứng")

Sau khi run game một lần trong thư mục resource sẽ tạo ra file setting SO cho Intertitial là AudienceAdsManager. Ta chỉnh lại các loại inter sẽ được show, khoảng delay giữa inter, reward theo yêu cầu

![Sửa lại config quảng cáo](./_Images/Step9_settingAdsConfig.PNG?raw=true "Sửa lại config quảng cáo")

Vào dashboard của game để điều chỉnh các thông số tương ứng https://dashboard.gamesontop.com/games. Thêm các vị trí show inter theo đúng id được gắn trong class enum vừa chỉnh sửa ở client

![Thêm các vị trí show inter theo đúng id được gắn trong enum ở client](./_Images/Step8_interPosition.PNG?raw=true "Thêm các vị trí show inter theo id")

## Cách sử dụng Ads Controller
Định nghĩa id các vị trí hiển thị inter (defaultInterstitialPositons) trong AudienceAdsManager SO
Gọi hàm show rewarded và truyền vào enum RewardedPositionType tương ứng đã được tạo từ trước
    AdsController.Instances.ShowRewardedVideo(() => {
        // Xử lý reward không show được
    },
    (gotRewarded) =>
    {
        if (gotRewarded)
        {
            // Xử lý trả thưởng thành công cho user
        }
        else
        {
            // Xử lý reward show được nhưng user không đủ điều kiện nhận thưởng
        }
    },
    rewardedPositionId,
    rewardedPositionName)

Gọi hàm show inter và truyền vào enum InterstitialPositionType tương ứng đã được tạo từ trước

    AdsController.Instances.ShowInterstitial(() => {
        // Xử lý sau khi đóng inter thành công
    }, int positionType);

Gọi hàm show banner

    AdsController.Instances.ShowBanner(bool isvisible)

## Cách sử dụng GameAnalytics

Log event lên firebase

    GameAnalytics.LogEventFirebase("event_name", new Parameter[] { })

Log event lên AppsFlyer

    GameAnalytics.LogEventAppsFlyer("event_name", new Dictionary<string, string>());
    //log trực tiếp ko có param
    GameAnalytics.LogFirebaseDirectly("event_name");

Log event gameplay: Trong code đã để sẵn hàm log gameplay cơ bản phục vụ tracking từ cả AppsFlyer và Firebase
    GameAnalytics.LogGamePlayData(int level, GAMEPLAY_STATE gameState, object param = null, string timeProgress = null)

    GAMEPLAY_STATE.WIN : trạng thái thắng của game
    GAMEPLAY_STATE.LOSE : trạng thái thua của game
    GAMEPLAY_STATE.START_LEVEL : trạng thái bắt đầu level
    GAMEPLAY_STATE.SKIP : trạng thái bỏ qua level

Log event IAP
    GameAnalytics.LogPurchase(UnityEngine.Purchasing.Product product, IAP_STATE iapState)

Log show rate us 
    GameAnalytics.LogRateUsShow()

Log show rate us 5 sao 
    GameAnalytics.LogRateUs5Stars()

Log bắt đầu tutorial 
    GameAnalytics.LogTutorialBegin()

Log kết thúc tutorial 
    GameAnalytics.LogTutorialComplete()

Log click button
    GameAnalytics.LogEventButton(string nameScreen, string nameButton)

## Cách sử dụng InAppPurchase

Tìm scriptable object IAPCatalogSO Điền đầy đủ thông tin các gói mua IAP trong game vào

![Sửa id trong IAPCatalogSO](./_Images/Step12_settingIAPCatalogSO.PNG?raw=true "Sửa id trong IAPCatalogSO")

Kéo prefabs MonsterIAPManager vào trong scene loading

![Kéo prefabs MonsterIAPManager vào trong scene loading](./_Images/Step13_addIAPManagerPrefabs.PNG?raw=true "Kéo prefabs MonsterIAPManager vào trong scene loading")

Ở các button yêu cầu mua IAP ta thêm Component MonsterIAPbutton vào, điều chỉnh lại id gói, text hiển thị giá và gắn các event xử lý mua thành công cũng như mua thất bại

![Chỉnh lại Component MonsterIAPbutton](./_Images/Step14_SettingIAPButton.PNG?raw=true "Chỉnh lại Component MonsterIAPbutton")
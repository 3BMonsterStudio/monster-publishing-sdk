public class RestAPIConstant
{

    #region Special Event Data
    public const string API_CONFIG_EVENT            = "/v4/games/events";
    public const string API_SAVE_EVENT_USER_DATA    = "/v4/games/events/savedata";
    public const string API_UPDATE_EVENT_USER_DATA  = "/v4/games/events/updatedata";
    public const string API_LOAD_EVENT_USER_DATA    = "/v4/games/events/getdata";
    public const string API_VIP_CONFIG              = "/v4/iap/vipconfig";
    public const string API_AUDIENCE                = "/v4/games/players/audiencewelfare";
    public const string API_IAP_VERIFY              = "/v4/iap/verify";
    public const string API_LOGIN                   = "/v4/games/services/login";
    public const string API_LOGOUT                  = "/v4/games/services/logout";
    public const string API_SWAP_LOGIN              = "/v4/games/services/swaplogin";
    public const string API_PLAYER_PROFILE          = "/v4/games/players/data";
    public const string API_GIFTCODE                = "/v4/games/giftcodes/redeem";
    public const string API_LOGIN_DATA              = "/v4/games/players/logindata";
    public const string API_PATH_CATEGORY           = "/v2/games/cn/library";
    #endregion

    #region API Param
    public const string PARAM_PLAYER_ID             = "playerId";
    public const string PARAM_PLAYER_NAME           = "playerName";
    public const string PARAM_LOGIN_TOKEN           = "loginToken";
    public const string PARAM_EVENT_ID              = "eventId";
    public const string PARAM_APP_VERSION           = "appVersion";
    public const string PARAM_TEST                  = "test";
    public const string PARAM_EVENT_USER_DATA       = "eventData";
    public const string PARAM_ORDER_ID              = "orderId";
    public const string PARAM_PURCHASE_TOKEN        = "purchaseToken";
    public const string PARAM_PRODUCT_ID            = "productId";
    public const string PARAM_PRODUCT_TYPE          = "productType";
    public const string PARAM_NONCE                 = "nonce";
    public const string PARAM_PACK_ID               = "packId";
    public const string PARAM_DEVICE_ID             = "deviceId";
    public const string PARAM_AVA_URL               = "avatarUrl";
    public const string PARAM_NAME                  = "name";
    public const string PARAM_HASH                  = "hash";
    public const string PARAM_TIME_STAMP            = "clientTimestamp";

    #endregion

}

class DigiSign {
    public string _id { get; set; } 
    public int order { get; set; } = 0;
    public int duration { get; set; } = 0;
    public string type { get; set; }
    public string feed { get; set; }
    public string name { get; set; }

    public string text { get; set; }

    public string uri { get; set; }
    public string format { get; set; }

    public string Icon { get {
        switch(type) {
            case "image":
                return "oi oi-image";
                break;
            case "hide":
                return "oi oi-ban";
                break;
            case "text":
                return "oi oi-align-left";
                break;
            case "base64image":
            case "base64":
                return "oi oi-code";
                break;
            case "video":
                return "oi oi-video";
                break;
            case "web":
                return "oi oi-globe";
                break;
            default:
                return "oi oi-question-mark";
        }
    }}
}
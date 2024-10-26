class ProviderMapper {
    constructor(name) {
        this.name = name;
    }

    getUri() {
        switch(this.name) {
            case "google":
                return "https://accounts.google.com/o/oauth2/auth";
            case "facebook":
                return "https://www.facebook.com/v18.0/dialog/oauth";
            default:
                return null;
        }
    }
}

module.exports = {
    ProviderMapper: ProviderMapper
}
using System.Collections.Generic;
using System.Text.Json;

namespace Types;

public class IdentityToken {
    public Dictionary<string, object> Payload { get; set; }
    public enum Claims {
        ISS,
        SUB,
        AUD,
        EXP,
        IAT
    };
    public IdentityToken() {
        this.Payload = new Dictionary<string, object>();
    }
    public IdentityToken(Dictionary<string, object> payload) {
        this.Payload = payload;
    }
    public bool HasClaim(IdentityToken.Claims claim) {
        return this.Payload.ContainsKey(claim.ToString().ToLower());
    }
    public bool HasClaim(string claim) {
        return this.Payload.ContainsKey(claim);
    }

    public bool HasClaims(IdentityToken.Claims[] claims) {
        foreach (IdentityToken.Claims claim in claims) {
            if (this.Payload.ContainsKey(claim.ToString().ToLower())) continue;
            return false;
        }
        return true;
    }

    public object GetClaim(IdentityToken.Claims claim) {
        string claimName = claim.ToString().ToLower();
        if (!this.Payload.ContainsKey(claimName)) return null;
        return this.Payload[claimName];
    }

    public object GetClaim(string claim) {
        if (!this.Payload.ContainsKey(claim)) return null;
        return this.Payload[claim];
    }
}
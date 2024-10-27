class TokenMemoryStore {
    constructor() {
        this.clients = {};
    }

    /**
     * This method stores timestamp associated with a client_id
     * It returns null if everything is fine, otherwise it returns an error message
     * @param {*} client_id 
     * @param {*} timestamp 
     * @returns 
     */
    store(client_id, timestamp) {
        const current = this.clients[client_id];
        if(current && current > timestamp) {
            return "Already used";
        }
        let now = Math.floor(new Date().getTime() / 1000);
        if (timestamp > now + 10) {
            return "Invalid timestamp";
        }
        this.clients[client_id] = timestamp;
        return null;
    }
}

module.exports = {
    TokenMemoryStore: TokenMemoryStore
}
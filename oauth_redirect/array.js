class PendingArray {
    constructor(length, timeout) {
        this.length = length;
        this.array = new Array(length);
        this.timeout = timeout;
        this.index = 0;
    }

    insert(key, client_uri) {
        const index = this.index;
        this.index = (this.index + 1) % this.length;
        this.array[index] = {
            key: key,
            client_uri: client_uri,
            timestamp: (new Date()).getTime() + this.timeout
        }
    }

    get(key) {
        const now = (new Date()).getTime();
        for (let i = 0; i < this.array.length; i++) {
            if(this.array[i] === undefined) {
                continue;
            }
            console.log(typeof this.array[i]);
            if (this.array[i].key === key && this.array[i].timestamp > now) {
                return {
                    client_uri: this.array[i].client_uri
                }
            }
        }
        return null;
    }
}

module.exports = {
    PendingArray: PendingArray
}
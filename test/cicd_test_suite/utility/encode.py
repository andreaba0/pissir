import base64
import hashlib
import hmac

class Encode:
    def base64url_encode(string):
        input_bytes = string.encode('utf-8')
        encoded_bytes = base64.b64encode(input_bytes)

        # Convert the encoded bytes to a string
        encoded_string = encoded_bytes.decode('utf-8')

        # Convert the string to Base64URL by replacing '+' with '-', '/' with '_', and removing '='
        base64url_string = encoded_string.replace('+', '-').replace('/', '_').rstrip('=')

        return base64url_string

    def hmac_sha256_encode(key, string):
        key_bytes = key.encode('utf-8')
        string_bytes = string.encode('utf-8')

        # Create a new HMAC object
        h = hmac.new(key_bytes, string_bytes, hashlib.sha256)

        # Return the encoded string
        return h.hexdigest()
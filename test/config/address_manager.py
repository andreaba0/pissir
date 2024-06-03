class address_manager:
    def __init__(self, addresses):
        self.address_pool = addresses

    def get_address(self):
        if self.pool_index < len(self.address_pool):
            address = self.address_pool[self.pool_index]
            self.pool_index += 1
            return address
        else:
            return None 
    
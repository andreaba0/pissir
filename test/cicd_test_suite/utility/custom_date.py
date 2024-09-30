import datetime
import re

class CustomDate:
    def __init__ (self, hour, minutes, seconds, day, month, year):
        self.hour = hour
        self.minutes = minutes
        self.seconds = seconds
        self.day = day
        self.month = month
        self.year = year

        # must contemplate also the time zone
        self.date = datetime.datetime(year, month, day, hour, minutes, seconds)
    
    def parse(str):
        match_pattern = r"^(?P<day>[0-9]+)-(?P<month>[0-9]+)-(?P<year>[0-9]+)T(?P<hour>[0-9]+):(?P<minutes>[0-9]+):(?P<seconds>[0-9]+)$"
        match = re.match(match_pattern, str)
        if match is None:
            return None
        return CustomDate(
            int(match.group("hour")),
            int(match.group("minutes")),
            int(match.group("seconds")),
            int(match.group("day")),
            int(match.group("month")),
            int(match.group("year"))
        )
    
    def __str__(self):
        return f"{self.date}"
    

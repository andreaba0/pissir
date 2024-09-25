class PostgresSuite:
    def clearDatabase(conn):
        cur = conn.cursor()
        cur.execute('''delete from company''')
        cur.close()
        conn.commit()
        conn.close()
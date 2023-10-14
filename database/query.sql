select partitaIvaAcquirente, sum(totale) - (select
    sum(esigenzaAcqua.totale) from esigenzaAcqua
    where esigenzaAcqua.partitaIva = acquisto.partitaIvaAcquirente
    group by esigenzaAcqua.partitaIva
) 
from acquisto
group by partitaIvaAcquirente
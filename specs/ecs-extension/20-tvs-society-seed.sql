-- Executed inside the TVS data-load transaction.
-- Source: TVS_ForTesting.mdb, Gen_Bank_Name (one row).
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM public.gen_bank_name WHERE brcode = '34-063-1429') THEN
        RAISE EXCEPTION 'LUCAS TVS BrCode already exists';
    END IF;
    IF EXISTS (SELECT 1 FROM public.gen_bank_name WHERE bank_id = 14290000001) THEN
        RAISE EXCEPTION 'LUCAS TVS bank_id already exists';
    END IF;

    INSERT INTO public.gen_bank_name
        (bank_id, bank_name, bank_address, bank_city, bank_address2,
         bank_shortname, bank_type, brcode)
    VALUES
        (14290000001,
         'LUCAS TVS EMPLOYEES CO-OP THRIFT AND CREDIT SOCIETY LTD,PADI,CHENNAI',
         'PADI', 'CHENNAI', 'CHENNAI', 'LUCAS TVS', 3, '34-063-1429');
END $$;

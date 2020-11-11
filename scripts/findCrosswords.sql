create or replace function find_crosswords (
   _offset integer, _length integer
) 
returns table ( cw_id integer, cw_name character varying, cw_theme integer, cw_owner character varying, cw_rathing float ) 
language plpgsql
as $$
declare 
-- variable declaration
BEGIN

	RETURN QUERY
	SELECT crosswords.crossword_id,
			crosswords.crossword_name,
			crosswords.crossword_theme,
			users_data.user_login,
			crosswords.crossword_rathing from crosswords
		INNER JOIN public.users_data
			ON crosswords.crossword_owner = users_data.user_id
			
ORDER BY crossword_rathing
			OFFSET _offset ROWS 
FETCH FIRST _length ROW ONLY; 
		
END; $$ ;

select * from find_crosswords(0, 2);

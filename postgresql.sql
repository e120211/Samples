declare
  __res jsonb;
  __err jsonb = core.get_error(1000);
  __data jsonb = _data::jsonb;
  __size integer = coalesce(__data->>'size', '10')::integer;
  __firstid uuid = coalesce((__data->>'firstid')::uuid, core.guid_null())::uuid;
  __lastid uuid = coalesce((__data->>'lastid')::uuid, core.guid_null())::uuid;
  __sort varchar = coalesce(__data->>'sort', 'sort_date_desc')::varchar;
  __dir varchar = coalesce(__data->>'dir', 'bottom')::varchar;
  __search varchar = coalesce(__data->>'search', null)::varchar;
  __deleted integer = coalesce(__data->>'deleted', '0')::integer;
begin

  with t as (
    select
      x.id_note, row_number() over(order by case when y.val = 1 then 1 else 0 end desc,
      case when __sort = 'sort_date_desc' then x.id_updated_date end desc,
      case when __sort = 'sort_date_asc' then x.id_updated_date end asc,
      case when __sort = 'sort_title_desc' then x.title end desc,
      case when __sort = 'sort_title_asc' then x.title end asc
      ) rn,
      coalesce(y.val, 0) pin, coalesce(f.val, 0) fav
    from notes.t_note x
    left join notes.t_note_pin y using(id_user, id_note)
    left join notes.t_note_fav f using(id_user, id_note)
    where x.id_user = _id_user and x.is_deleted = __deleted
    and (__search is null or (x.title ~* __search or x.description ~* __search))
  ),
  r as (
    select t.id_note, t.rn, t.pin, t.fav, count(*) over() cnt from t
    order by t.rn asc
    limit __size offset case when __lastid is null or __lastid = core.guid_null() then 0 else (select distinct t.rn from t where t.id_note = __lastid limit 1 offset 0) end
  )
  select
  jsonb_agg(
    jsonb_build_object(
      'rn', r.rn,
      'total', r.cnt,
      'id_note', r.id_note,
      'created_date', x.id_created_date,
      'updated_date', x.id_updated_date,
      'title', x.title,
      'description', x.description,
      'param', x.param,
      'pin', r.pin,
      'fav', r.fav,
      'deleted', x.is_deleted
    )
  ) into __res
  from r
  left join notes.t_note x using(id_note)
  ;

  return jsonb_build_object('data', coalesce(__res, '[]'::jsonb), 'status', __err);
exception when OTHERS then
  return jsonb_build_object('data', null, 'status', core.get_error(1111, SQLERRM));
end;
CREATE VIEW sample_activities AS
WITH all_samples AS (
  SELECT
    s.id as sample_id,
    s.lab_number,
    s.barcode_number,
    ss.id as sample_set_id,
    g.id as grower_id,
    b.id as submitter_id,
    r.id as retailer_id
  FROM
    samples as s
    INNER JOIN sample_sets as ss ON s.sample_set_id = ss.id
    INNER JOIN growers as g ON ss.grower_id = g.id
    INNER JOIN submitters as b ON g.submitter_id = b.id
    INNER JOIN retailers as r ON b.retailer_id = r.id
  WHERE
    NOT s.status = 3
    AND (ss.day_soak <= DATE_PART('day', now() - ss.resin_date))
)
,all_sample_notes AS (
  SELECT
    i_s.lab_number,
    i_s.barcode_number,
    i_s.sample_id,
    i_s.sample_set_id,
    i_s.grower_id,
    i_s.submitter_id,
    i_s.retailer_id,
    COALESCE(n.id,0) as note_id
  FROM
    all_samples as i_s
    LEFT JOIN notes as n
      ON i_s.sample_id = n.sample_id
)
,sample_changes AS (
  SELECT
    v.id,
    s.lab_number,
    s.barcode_number,
    v.event,
    v.object_changes,
    v.item_type,
    s.sample_id,
    s.sample_set_id,
    s.grower_id,
    s.submitter_id,
    s.retailer_id,
    s.note_id,
    to_char(v.created_at,'MM/DD/YY HH:MI AM') as modified_on,
    u.first_name || ' ' || u.last_name as modified_by,
      v.created_at
  FROM
    versions as v
    INNER JOIN all_sample_notes as s ON (
      CASE v.item_type
        WHEN 'Retailer' THEN v.item_id = s.retailer_id
        WHEN 'Submitter' THEN v.item_id = s.submitter_id
        WHEN 'Grower' THEN v.item_id = s.grower_id
        WHEN 'SampleSet' THEN v.item_id = s.sample_set_id
        WHEN 'Sample' THEN v.item_id = s.sample_id
        WHEN 'Note' THEN v.item_id = s.note_id
        ELSE 1 = 0
      END
      AND NOT v.event = 'create'
    )
    INNER JOIN users as u
      ON CAST(v.whodunnit as INT) = u.id
)
SELECT
  row_number() OVER (ORDER BY created_at DESC) as id,
  lab_number,
  barcode_number,
  CASE
    WHEN item_type = 'Sample' AND ( object_changes::jsonb ? 'status') THEN
      CASE
        WHEN object_changes::text Like '%"status": %, "clean"]%' THEN 'Sample Batch Cleaning'
        ELSE ''
      END
    ELSE initcap(event) || ' ' || item_type || ' on sample'
  END || ' -- ' || object_changes::text as "activity",
  sample_id,
  sample_set_id,
  grower_id,
  submitter_id,
  retailer_id,
  note_id,
  modified_on,
  modified_by
FROM
  sample_changes

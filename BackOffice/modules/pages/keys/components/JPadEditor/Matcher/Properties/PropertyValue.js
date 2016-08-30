import React from 'react';
import ClosedComboBox from '../../../../../../components/common/ClosedComboBox/ClosedComboBox';
import style from './styles.css';
import { WithContext as ReactTags } from 'react-tag-input';
import R from 'ramda';

const ClosedComboBoxPropertyValue = ({ onUpdate, allowedValues, value }) =>
  (<div>
    <ClosedComboBox
      inputProps={{ onChange: ({ value }) => onUpdate(value), value }}
      suggestions={allowedValues}
      />
  </div>);

const TagsPropertyValue = ({ onUpdate, value }) => {
  let indexedTags = value.map(x => ({ id: x, text: x }));

  const handleAddtion = (newValue) => onUpdate([...value, newValue]);
  const handleDelete = (valueIndex) => onUpdate(R.remove(valueIndex, 1, value));

  return (<div>
    <label className={style['wrapping-bracet']}>[</label><ReactTags tags={ indexedTags }
      handleAddition={handleAddtion}
      handleDelete={handleDelete}
      placeholder="add value"
      allowDeleteFromEmptyInput
      classNames={{
        tags: style['tags-container'],
        tagInput: style['tag-input'],
        tag: style['tag'],
        remove: style['tag-delete-button'],
        suggestions: style['tags-suggestion'],
      } }
      /><label className={style['wrapping-bracet']}>]</label>
  </div>);
};

const InputPropertyValue = ({ onUpdate, value }) =>
  (<input className={style['value-input']} type="text" onChange={(e) => onUpdate(e.target.value) } value={value} />);

function PropertyValueComponent({ onUpdate, meta, value, op }) {
  if (meta.allowedValues) return (<ClosedComboBoxPropertyValue onUpdate={onUpdate} allowedValues={meta.allowedValues} value={value}/>);
  if (meta.multipleValues && op === '$in') { return (<TagsPropertyValue onUpdate={onUpdate} value={value}/>); }
  return (<InputPropertyValue onUpdate={onUpdate} value={value} />);
}

export default (props) => <div className={style['property-value-wrapper']}>
  <PropertyValueComponent {...props} />
</div>;
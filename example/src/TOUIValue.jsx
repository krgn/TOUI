import React, { PropTypes } from 'react';
import Panel from 'muicss/lib/react/panel';
import Appbar from 'muicss/lib/react/appbar';
import Input from 'muicss/lib/react/input';
import Checkbox from 'muicss/lib/react/checkbox';
import Dropdown from 'muicss/lib/react/dropdown';
import DropdownItem from 'muicss/lib/react/dropdown-item';

export const ValueTypes = {
  Boolean: 'boolean',
  String: 'string',
  Number: 'number',
  Color: 'color',
  Enum: 'enum',
  Array: 'array'
}

export const TOUIValueType = PropTypes.shape({
  pow2: PropTypes.bool,  
  unit: PropTypes.string,
  cyclic: PropTypes.bool,
  min: PropTypes.number,   
  max: PropTypes.number,  
  precision: PropTypes.number,  
  step: PropTypes.number,
  alpha: PropTypes.bool,
  behavior: PropTypes.number,
  default: PropTypes.any,
  name: PropTypes.string.isRequired
})

export const TOUIValue = PropTypes.shape({
  id: PropTypes.string.isRequired,
  value: PropTypes.any.isRequired,
  default: PropTypes.any,
  label: PropTypes.string,
  description: PropTypes.string,
  group: PropTypes.string,
  userdata: PropTypes.any,
  type: PropTypes.oneOfType([
    TOUIValueType,
    PropTypes.shape({
      name: PropTypes.string.isRequired,
      subtype: TOUIValueType
    })
  ]),
  filemask: PropTypes.string,
  maxChars: PropTypes.string,
  multiline: PropTypes.bool,
  entries: PropTypes.arrayOf(PropTypes.string)
})

const str2color = str => {
  let parse = part => {
    let hex = parseInt(part,10).toString(16)
    if( hex.length === 1 ) {
      return "0" + hex
    } else {
      return hex
    }
  }
  return "#" + str.split(',').map(parse).join("")
}

const BoolValue = ({ key, value, orig, onChange }) => (
  <Checkbox
      key={key}
      label={value.label}
      name={value.id}
      value={value.value}
      checked={value.value ? "checked" : null}
      onChange={(el) => {
        if(orig == null) {
          onChange(value.id, el.target.checked)
        } else {
          let values = Object.assign([],orig.value)
          values[key] = el.target.checked
          onChange(value.id, values)
        } 
      }} />
)

const BoolBox = ({ value, onChange }) => (
  <div>
    <h1>{value.label}</h1>
    <div>
      Type:
      <em> Bool</em> 
    </div>
    <div>
      Description:
      <em>{value.description}</em>
    </div>
    <div>
      User Data:
      <em>{value.userdata}</em>
    </div>
    <div>
      Value:
      <BoolValue key={0} value={value} onChange={onChange} />
    </div>
  </div>
)

const StringValue = ({ key, value, orig, onChange }) => (
  <Input
      key={key}
      hint={value.description}
      value={value.value}
      type="text"
      onChange={el => {
        if(orig == null) {
          onChange(value.id, el.target.value)
        } else {
          let values = Object.assign([],orig.value)
          values[key] = el.target.value
          onChange(value.id, values)
        } 
      }} />
)

const StringBox = ({ value, onChange }) => (
  <div>
    <h1>{value.label}</h1>
    <div>
      Type:
      <em> String</em> 
    </div>
    <div>
      Description:
      <em>{value.description}</em>
    </div>
    <div>
      Value:
      <StringValue key={0} value={value} onChange={onChange} />
    </div>
  </div>
)

const NumberValue = ({ key, value, orig, onChange }) => (
  <input
      key={key}
      value={value.value}
      type="number"
      onChange={el => {
          let parsed = parseFloat(el.target.value,10)
          if(orig == null) {
            onChange(value.id, parsed ? parsed : 0)
          } else {
            let values = Object.assign([],orig.value)
            values[key] = parsed ? parsed : 0
            onChange(value.id, values)
          }
      }} />
)

const NumberBox = ({ value, onChange }) => (
  <div>
    <h1>{value.label}</h1>
    <div>
      Type:
      <em >Number</em> 
    </div>
    <div>
      Description:
      <em>{value.description}</em>
    </div>
    <div>
      Value:
      <div className="mui-textfield">
        <NumberValue key={0} value={value} onChange={onChange} />
      </div>
    </div>
  </div>
)

const ColorValue = ({ key, value, orig, onChange }) => (
  <input
      key={key}
      value={str2color(value.value)}
      type="color"
      onChange={el => {
        if(orig == null) {
          onChange(value.id, el.target.value)
        } else {
          let values = Object.assign([],orig.value)
          values[key] = el.target.value
          onChange(value.id, values)
        } 
      }} />
)

const ColorBox = ({ value, onChange }) => (
  <div>
    <h1>{value.label}</h1>
    <div>
      Type:
      <em> Color</em> 
    </div>
    <div>
      Description:
      <em> {value.description}</em>
    </div>
    <div>
      User Data: 
      <em> {value.userdata}</em>
    </div>
    <div>
      Value:
      <div className="mui-textfield">
        <ColorValue key={0} value={value} onChange={onChange} />
      </div>
    </div>
  </div>
)

const EnumValue = ({ key, value, orig, onChange }) => (
  <select
      key={key}
      value={value.type.entries[value.value]}
      onChange={el => {
        if(orig == null) {
          onChange(value.id, value.type.entries.indexOf(el.target.value))
        } else {
          let values = Object.assign([],orig.value)
          values[key] = value.type.entries.indexOf(el.target.value)
          onChange(value.id, values)
        }
      }}>
    {
      value.type.entries.map(entry => {
        return <option key={entry} value={entry}>{entry}</option>
      })
    }
  </select>
)

const EnumBox = ({ value, onChange }) => (
  <div>
    <h1>{value.label}</h1>
    <div>
      Type:
      <em> Enum</em> 
    </div>
    <div>
      Description:
      <em> {value.description}</em>
    </div>
    <div>
      <div>Value:</div>
      <EnumValue
        key={0}
        value={value}
        onChange={onChange}/>
    </div>
  </div>
)

const ArrayBox = ({ value, onChange }) => (
  <div>
    <h1>{value.label}</h1>
    <div>
      Type:
      <em> {"Array<" + value.type.subtype.name + ">"}</em> 
    </div>
    <div>
      Description:
      <em> {value.description}</em>
    </div>
    <div>
      <div>Value:</div>
      {
        value.value.map((val, idx) => {
          let merged = Object.assign({}, value, {
            type: value.type.subtype,
            value: val
          })
          switch(value.type.subtype.name) {
            case ValueTypes.Boolean:
              return BoolValue({ key: idx, value: merged, orig: value, onChange: onChange })
            case ValueTypes.String:
              return StringValue({ key: idx, value: merged, orig: value, onChange: onChange })
            case ValueTypes.Number:
              return NumberValue({ key: idx, value: merged, orig: value, onChange: onChange })
            case ValueTypes.Color:
              return ColorValue({ key: idx, value: merged, orig: value, onChange: onChange })
            case ValueTypes.Enum:
              return EnumValue({ key: idx, value: merged, orig: value, onChange: onChange })
          }
        })
      }
    </div>
  </div>
) 

function renderValue(props) {
  switch(props.value.type.name) {
    case ValueTypes.Boolean:
      return BoolBox(props)
    case ValueTypes.String:
      return StringBox(props)
    case ValueTypes.Number:
      return NumberBox(props)
    case ValueTypes.Color:
      return ColorBox(props)
    case ValueTypes.Enum:
      return EnumBox(props)
    case ValueTypes.Array:
      console.log("array:", props)
      return ArrayBox(props)
    default:
      console.log("unknown type:", props.value.type.name)
  }
}

export const ValueWidget = (props) => (
  <Panel id={props.value.id}>
    {renderValue(props)}
  </Panel>
)

ValueWidget.propTypes = {
  value: TOUIValue,
  onChange: PropTypes.func.isRequired
}

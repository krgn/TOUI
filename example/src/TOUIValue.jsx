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
  Enum: 'enum'
}

export const TOUIValue = PropTypes.shape({
  id: PropTypes.string.isRequired,
  value: PropTypes.any.isRequired,
  default: PropTypes.any,
  label: PropTypes.string,
  description: PropTypes.string,
  group: PropTypes.string,
  userdata: PropTypes.any,
  type: PropTypes.shape({
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
    name: PropTypes.string
  }),
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

const BoolValue = ({ value, onChange }) => (
  <div>
    <h1>{value.label}</h1>
    <div>
      Type:
      <em>Bool</em> 
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
      <Checkbox
          label={value.label}
          name={value.id}
          value={value.value}
          checked={value.value ? "checked" : null}
          onChange={(el) =>  
            onChange(value.id,el.target.checked) 
          } />
    </div>
  </div>
)

const StringValue = ({ value, onChange }) => (
  <div>
    <h1>{value.label}</h1>
    <div>
      Type:
      <em>String</em> 
    </div>
    <div>
      Description:
      <em>{value.description}</em>
    </div>
    <div>
      Value:
      <Input
          hint={value.description}
          value={value.value}
          type="text"
          onChange={el => onChange(value.id, el.target.value)} />
    </div>
  </div>
)

const NumberValue = ({ value, onChange }) => (
  <div>
    <h1>{value.label}</h1>
    <div>
      Type:
      <em>Number</em> 
    </div>
    <div>
      Description:
      <em>{value.description}</em>
    </div>
    <div>
      Value:
      <div className="mui-textfield">
        <input
            value={value.value}
            type="number"
            onChange={el => {
              let val = parseFloat(el.target.value,10)
              onChange(value.id, val ? val : 0)
            }} />
      </div>
    </div>
  </div>
)

const ColorValue = ({ value, onChange }) => (
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
        <input
            value={str2color(value.value)}
            type="color"
            onChange={el => onChange(value.id, el.target.value)} />
      </div>
    </div>
  </div>
)

const EnumValue = ({ value, onChange }) => (
  <div>
    <h1>{value.label}</h1>
    <div>
      Type:
      <em>Enum</em> 
    </div>
    <div>
      Description:
      <em>{value.description}</em>
    </div>
    <div>
      <div>Value:</div>
      <select onChange={el => onChange(value.id, el.target.value)}>
        <option value="1">1</option>
        <option value="2">2</option>
        <option value="3">3</option>
      </select>
    </div>
  </div>
)

function renderValue(props) {
  switch(props.value.type.name) {
    case ValueTypes.Boolean:
      return BoolValue(props)
    case ValueTypes.String:
      return StringValue(props)
    case ValueTypes.Number:
      return NumberValue(props)
    case ValueTypes.Color:
      return ColorValue(props)
    case ValueTypes.Enum:
      return EnumValue(props)
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

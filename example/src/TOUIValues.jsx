import React, { PropTypes } from 'react';
import { Actions } from './State';
import { connect } from 'react-redux';
import { ValueWidget, TOUIValue } from './TOUIValue';

const ValueList = ({ values, onChange }) => (
  <div>
    {Object.keys(values).map(key => {
      let value = values[key];
      return <ValueWidget
                 key={key}
                 value={value}
                 onChange={onChange} />
    })}
  </div>
) 

ValueList.propTypes = {
  values: PropTypes.object.isRequired,
  onChange: PropTypes.func.isRequired
}

const mapStateToProps = (state) => {
  return { values: state.values }
}

const mapDispatchToProps = (dispatch) => {
  return {
    onChange: (id, payload) => {
      dispatch({
        type: Actions.ClientUpdate,
        payload: { id: id, value: payload }
      })
    }
  }
}

const TOUIValues = connect(
  mapStateToProps,
  mapDispatchToProps 
)(ValueList)

export default TOUIValues

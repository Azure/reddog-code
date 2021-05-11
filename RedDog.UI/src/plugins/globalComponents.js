import { BaseInput, Card, BaseDropdown, BaseButton, BaseCheckbox } from "../components/index";

const GlobalComponents = {
  install(Vue) {
    Vue.component(BaseInput.name, BaseInput);
    Vue.component(Card.name, Card);
    Vue.component(BaseDropdown.name, BaseDropdown);
    Vue.component(BaseButton.name, BaseButton);
    Vue.component(BaseCheckbox.name, BaseCheckbox);
  }
};

export default GlobalComponents;
